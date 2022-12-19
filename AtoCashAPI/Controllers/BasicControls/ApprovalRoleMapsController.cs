using System;
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
    [Route("api/[controller]")]
    [ApiController]
      [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User")]
    public class ApprovalRoleMapsController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public ApprovalRoleMapsController(AtoCashDbContext context)
        {
            _context = context;
        }

        // GET: api/ApprovalRoleMaps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApprovalRoleMapDTO>>> GetApprovalRoleMaps()
        {
            List<ApprovalRoleMapDTO> ListApprovalRoleMapDTO = new List<ApprovalRoleMapDTO>();

            //var approvalRoleMaps = await _context.ApprovalRoleMaps.ToListAsync();

            //foreach (ApprovalRoleMap approvalRoleMap in approvalRoleMaps)
            //{
            //    ApprovalRoleMapDTO approvalRoleMapDTO = new();


            //    approvalRoleMapDTO.Id = approvalRoleMap.Id;
            //    approvalRoleMapDTO.ApprovalGroupId = approvalRoleMap.ApprovalGroupId;
            //    approvalRoleMapDTO.ApprovalGroup = _context.ApprovalGroups.Find(approvalRoleMap.ApprovalGroupId).ApprovalGroupCode;
            //    approvalRoleMapDTO.JobRoleId = approvalRoleMap.JobRoleId;
            //    approvalRoleMapDTO.JobRole = _context.JobRoles.Find(approvalRoleMap.JobRoleId).JobRoleCode;
            //    approvalRoleMapDTO.ApprovalLevelId = approvalRoleMap.ApprovalLevelId;
            //    approvalRoleMapDTO.ApprovalLevel = _context.ApprovalLevels.Find(approvalRoleMap.ApprovalLevelId).Level;


            ////    int empCount = _context.Employees.Where(e => (e.ApprovalGroupId == approvalRoleMap.ApprovalGroupId || e.StoreApprovalGroupId == approvalRoleMap.ApprovalGroupId) && (e.JobRoleId == approvalRoleMap.JobRoleId || e.StoreRoleId == approvalRoleMap.JobRoleId)).Count();
            // //   var employeeAssigned = _context.Employees.Where(e => (e.ApprovalGroupId == approvalRoleMap.ApprovalGroupId || e.StoreApprovalGroupId == approvalRoleMap.ApprovalGroupId) && (e.JobRoleId == approvalRoleMap.JobRoleId || e.StoreRoleId == approvalRoleMap.JobRoleId)).FirstOrDefault();

            //    string empName = string.Empty;
            ////    int ApprvLevel = _context.ApprovalLevels.Find(approvalRoleMap.ApprovalLevelId).Level;
            //    if (ApprvLevel != 1)
            //    {
            //        if (employeeAssigned != null)
            //        {
            //            empName = employeeAssigned.GetFullName();
            //        }
            //        else
            //        {
            //            empName = "Un-Assigned";
            //        }
            //    }
            //    else
            //    {

            //        if (empCount > 1)
            //        {
            //            if (employeeAssigned != null)
            //            {
            //                empName = "Assigned to =" + empCount;
            //            }

            //        }
            //        else if(empCount == 1)
            //        {
            //            empName = employeeAssigned.GetFullName();
            //        }
            //         else
            //        {
            //            empName = "Un-Assigned";
            //        }
            //    }
            //    approvalRoleMapDTO.EmployeeName = empName;


            //    ListApprovalRoleMapDTO.Add(approvalRoleMapDTO);

            //}

            return ListApprovalRoleMapDTO;
        }

        // GET: api/ApprovalRoleMaps/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApprovalRoleMapDTO>> GetApprovalRoleMap(int id)
        {
            ApprovalRoleMapDTO approvalRoleMapDTO = new ApprovalRoleMapDTO();

            var approvalRoleMap = await _context.ApprovalRoleMaps.FindAsync(id);

            if (approvalRoleMap == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Approval Rolemap Id invalid!" });
            }

            approvalRoleMapDTO.Id = approvalRoleMap.Id;
            approvalRoleMapDTO.ApprovalGroupId = approvalRoleMap.ApprovalGroupId;
            approvalRoleMapDTO.JobRoleId = approvalRoleMap.JobRoleId;
            approvalRoleMapDTO.ApprovalLevelId = approvalRoleMap.ApprovalLevelId;

            return approvalRoleMapDTO;
        }

        // PUT: api/ApprovalRoleMaps/5
        [HttpPut("{id}")]
          [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutApprovalRoleMap(int id, ApprovalRoleMapDTO approvalRoleMapDto)
        {
            if (id != approvalRoleMapDto.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }


            var approvalRoleMap = await _context.ApprovalRoleMaps.FindAsync(id);

            approvalRoleMap.Id = approvalRoleMapDto.Id;
            approvalRoleMap.ApprovalGroupId = approvalRoleMapDto.ApprovalGroupId;
            approvalRoleMap.JobRoleId = approvalRoleMapDto.JobRoleId;
            approvalRoleMap.ApprovalLevelId = approvalRoleMapDto.ApprovalLevelId;

            _context.ApprovalRoleMaps.Update(approvalRoleMap);
            _context.Entry(approvalRoleMap).State = EntityState.Modified;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApprovalRoleMapExists(id))
                {
                    return Conflict(new RespStatus { Status = "Failure", Message = "RoleMap is invalid" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new RespStatus { Status = "Success", Message = "Role Map Updated!" });
        }

        // POST: api/ApprovalRoleMaps
        [HttpPost]
          [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<ApprovalRoleMap>> PostApprovalRoleMap(ApprovalRoleMapDTO approvalRoleMapDto)
        {
            var AprvRolMap = _context.ApprovalRoleMaps.Where(a => a.ApprovalGroupId == approvalRoleMapDto.ApprovalGroupId && a.JobRoleId == approvalRoleMapDto.JobRoleId && a.ApprovalLevelId == approvalRoleMapDto.ApprovalLevelId).FirstOrDefault();
            if (AprvRolMap != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Approval Role Map already exists" });
            }

            var approvalgroup = _context.ApprovalRoleMaps.Where(a => a.ApprovalGroupId == approvalRoleMapDto.ApprovalGroupId).ToList();
            int maxApprLevel = 0;

            //if (approvalgroup.Count >0)
            //{
            //     maxApprLevel = _context.ApprovalRoleMaps.Where(a => a.ApprovalGroupId == approvalRoleMapDto.ApprovalGroupId).OrderByDescending(a => a.ApprovalLevelId).First().ApprovalLevelId;
            //}

            if (approvalRoleMapDto.ApprovalLevelId != maxApprLevel + 1)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Assign only in Linear Increasing Order" });
            }

            //Check for Duplicate Levels in the same group
            AprvRolMap = _context.ApprovalRoleMaps.Where(a => a.ApprovalGroupId == approvalRoleMapDto.ApprovalGroupId && a.ApprovalLevelId == approvalRoleMapDto.ApprovalLevelId).FirstOrDefault();
            if (AprvRolMap != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Group Duplicate Approval Levels are Not allowed !" });
            }

            ApprovalRoleMap approvalRoleMap = new ApprovalRoleMap
            {

                ApprovalGroupId = approvalRoleMapDto.ApprovalGroupId,
                JobRoleId = approvalRoleMapDto.JobRoleId,
                ApprovalLevelId = approvalRoleMapDto.ApprovalLevelId
            };

            _context.ApprovalRoleMaps.Add(approvalRoleMap);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Approval To Role... Mapped!" });
        }

        // DELETE: api/ApprovalRoleMaps/5
        [HttpDelete("{id}")]
          [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteApprovalRoleMap(int id)
        {
            var approvalRoleMap = await _context.ApprovalRoleMaps.FindAsync(id);
            if (approvalRoleMap == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Approval Role Map Id invalid!" });
            }

            _context.ApprovalRoleMaps.Remove(approvalRoleMap);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Approval Role Map Deleted!" });
        }

        private bool ApprovalRoleMapExists(int id)
        {
            return _context.ApprovalRoleMaps.Any(e => e.Id == id);
        }
    }
}
