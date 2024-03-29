﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtoCashAPI.Data;
using AtoCashAPI.Models;
using Microsoft.AspNetCore.Authorization;
using AtoCashAPI.Authentication;

namespace AtoCashAPI.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, Manager, User")]
    public class ProjectsController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public ProjectsController(AtoCashDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        [ActionName("ProjectsForDropdown")]
        public async Task<ActionResult<IEnumerable<ProjectVM>>> GetProjectsForDropDown()
        {
            List<ProjectVM> ListProjectVM = new();

            var projects = await _context.Projects.Where(p => p.StatusTypeId == (int)EStatusType.Active).ToListAsync();
            foreach (Project project in projects)
            {
                ProjectVM projectVM = new()
                {
                    Id = project.Id,
                    ProjectName = project.ProjectName + ":" + project.ProjectDesc
                };

                ListProjectVM.Add(projectVM);
            }

            return ListProjectVM;

        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetProjects()
        {
            List<ProjectDTO> ListProjectDTO = new();

            var projects = await _context.Projects.ToListAsync();

            foreach (Project project in projects)
            {
                Employee? emp = _context.Employees.Find(project.ProjectManagerId);
                StatusType? statusType = _context.StatusTypes.Find(project.StatusTypeId);

                ProjectDTO projectDTO = new()
                {
                    Id = project.Id,
                    ProjectName = project.ProjectName,
                    CostCenterId = project.CostCenterId,
                    ProjectDesc = project.ProjectDesc,
                    StatusTypeId = project.StatusTypeId,
                    ProjectManager = emp == null ? "" : emp.GetFullName(),
                    StatusType = statusType == null ? "" : statusType.Status
                };

                ListProjectDTO.Add(projectDTO);

            }

            return ListProjectDTO;
        }



        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDTO>> GetProject(int id)
        {
            var proj = await _context.Projects.FindAsync(id);

            if (proj == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Project Id is Invalid!" });
            }

            StatusType? statusType = _context.StatusTypes.Find(proj.StatusTypeId);
            ProjectDTO projectDTO = new()
            {
                Id = proj.Id,
                ProjectName = proj.ProjectName,
                CostCenterId = proj.CostCenterId,
                ProjectManagerId = proj.ProjectManagerId,
                ProjectDesc = proj.ProjectDesc,
                StatusTypeId = proj.StatusTypeId,
                StatusType = statusType == null ? "" : statusType.Status
            };

            return projectDTO;

        }

        // GET: api/ProjectManagement/5
        [HttpGet("{id}")]
        [ActionName("GetEmployeeAssignedProjects")]
        public ActionResult<ProjectVM> GetEmployeeAssignedProjects(int id)
        {
            var listOfProjmgts = _context.ProjectManagements.Where(p => p.EmployeeId == id).ToList();

            List<ProjectVM> ListprojectVM = new();

            if (listOfProjmgts != null)
            {
                foreach (var item in listOfProjmgts)
                {
                    if (_context.Projects.Find(item.ProjectId).StatusTypeId == (int)EStatusType.Active)
                    {
                        ProjectVM project = new()
                        {
                            Id = item.ProjectId,
                            ProjectName = _context.Projects.Find(item.ProjectId).ProjectName
                        };
                        ListprojectVM.Add(project);
                    }
                }
                return Ok(ListprojectVM);
            }
            return Ok(new RespStatus { Status = "Success", Message = "No Projects Assigned to Employee" });
        }

        // PUT: api/Projects/5
        [HttpPut("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutProject(int id, ProjectDTO projectDto)
        {
            if (id != projectDto.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }

            var proj = await _context.Projects.FindAsync(id);

            proj.Id = projectDto.Id;
            proj.ProjectName = projectDto.ProjectName;
            proj.CostCenterId = projectDto.CostCenterId;
            proj.ProjectManagerId = projectDto.ProjectManagerId;
            proj.ProjectDesc = projectDto.ProjectDesc;
            proj.StatusTypeId = projectDto.StatusTypeId;

            _context.Projects.Update(proj);
            //_context.Entry(projectDto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return Conflict(new RespStatus { Status = "Failure", Message = "Project is invalid" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new RespStatus { Status = "Success", Message = "Project Details Updated!" });
        }

        // POST: api/Projects
        [HttpPost]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<Project>> PostProject(ProjectDTO projectDto)
        {
            var project = _context.Projects.Where(c => c.ProjectName == projectDto.ProjectName).FirstOrDefault();
            if (project != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "ProjectName already exists" });
            }

            Project proj = new()
            {
                ProjectName = projectDto.ProjectName,
                CostCenterId = projectDto.CostCenterId,
                ProjectManagerId = projectDto.ProjectManagerId,
                ProjectDesc = projectDto.ProjectDesc,
                StatusTypeId = projectDto.StatusTypeId
            };

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {

                _context.Projects.Add(proj);
                await _context.SaveChangesAsync();
                //Add Project manager to the Project by default
                ProjectManagement projectManagement = new();
                projectManagement.EmployeeId = projectDto.ProjectManagerId;
                projectManagement.ProjectId = proj.Id;

                _context.ProjectManagements.Add(projectManagement);

                await _context.SaveChangesAsync();

                await AtoCashDbContextTransaction.CommitAsync();
            }
            return Ok(new RespStatus { Status = "Success", Message = "Project Created!" });
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var subProj = _context.SubProjects.Where(s => s.ProjectId == id).FirstOrDefault();
            if (subProj != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Cant Delete the Project in Use" });
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Project Id is Invalid!" });
            }

            bool blnUsedInTravelReq = _context.TravelApprovalRequests.Where(t => t.ProjectId == id).Any();
            bool blnUsedInCashAdvReq = _context.CashAdvanceRequests.Where(t => t.ProjectId == id).Any();
            bool blnUsedInExpeReimReq = _context.ExpenseReimburseRequests.Where(t => t.ProjectId == id).Any();

            if (blnUsedInTravelReq || blnUsedInCashAdvReq || blnUsedInExpeReimReq)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Project in Use, Cant delete!" });
            }

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {
                _context.Projects.Remove(project);

                List<ProjectManagement> ProjMgmtItems = await _context.ProjectManagements.Where(p => p.ProjectId == id).ToListAsync();
                _context.ProjectManagements.RemoveRange(ProjMgmtItems);
                await _context.SaveChangesAsync();

                await AtoCashDbContextTransaction.CommitAsync();
            }
            return Ok(new RespStatus { Status = "Success", Message = "Project Deleted!" });
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }



        //
    }
}
