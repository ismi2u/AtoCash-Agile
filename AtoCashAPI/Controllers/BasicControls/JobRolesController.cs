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
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User")]
    public class JobRolesController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public JobRolesController(AtoCashDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ActionName("JobRolesQuery")]
        public async Task<ActionResult<IEnumerable<JobRoleVM>>> JobRolesForDeptDropdown()
        {
            List<JobRoleVM> ListJobRoleVM = new();

            //var jobRoles = await _context.JobRoles.Where(j => j.IsStoreRole == false).ToListAsync();
            var jobRoles = await _context.JobRoles.ToListAsync();
            foreach (JobRole jobRole in jobRoles)
            {
                JobRoleVM jobRoleVM = new()
                {
                    Id = jobRole.Id,
                    JobRoleCode = jobRole.JobRoleCode + ":" + jobRole.JobRoleName
                };

                ListJobRoleVM.Add(jobRoleVM);
            }

            return ListJobRoleVM;

        }


        [HttpGet("{isStoreRole}")]
        [ActionName("JobRolesQuery")]
        public async Task<ActionResult<IEnumerable<JobRoleVM>>> JobRolesForDeptDropdown(bool isStoreRole)
        {
            List<JobRoleVM> ListJobRoleVM = new();

            var jobRoles = await _context.JobRoles.ToListAsync();

            foreach (JobRole jobRole in jobRoles)
            {
                JobRoleVM jobRoleVM = new()
                {
                    Id = jobRole.Id,
                    JobRoleCode = jobRole.JobRoleCode + ":" + jobRole.JobRoleName
                };

                ListJobRoleVM.Add(jobRoleVM);
            }

            return ListJobRoleVM;

        }


        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobRole>>> GetRoles()
        {
            return await _context.JobRoles.ToListAsync();
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobRole>> GetRole(int id)
        {
            var role = await _context.JobRoles.FindAsync(id);

            if (role == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Role Id is invalid!" });
            }

            return role;
        }

        // PUT: api/Roles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutRole(int id, JobRoleDTO jobrole)
        {
            if (id != jobrole.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }

            JobRole? jobRole = _context.JobRoles.Find(jobrole.Id ?? 0);

            var jRole = await _context.JobRoles.FindAsync(id);

            try
            {
                if (jRole != null)
                {
                    jRole.JobRoleName = jobrole.JobRoleName == null ? "" : jobrole.JobRoleName;
                    jRole.MaxPettyCashAllowed = jobrole.MaxPettyCashAllowed;
                    _context.JobRoles.Update(jRole);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "JobRole cannot be updated!" });
            }

            return Ok(new RespStatus { Status = "Success", Message = "JobRole Updated!" });
        }

        // POST: api/Roles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<JobRole>> PostRole(JobRole role)
        {
            var jRole = _context.JobRoles.Where(c => c.JobRoleCode == role.JobRoleCode).FirstOrDefault();
            if (jRole != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "JobRole already exists" });
            }
            _context.JobRoles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "JobRole Created!" });
        }

        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.JobRoles.FindAsync(id);
            if (role == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "JobRole Id is Invalid!" });
            }

            if (_context.EmployeeExtendedInfos.Where(x => x.JobRoleId == id).Any())
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "JobRole is in Use!" });
            }

            _context.JobRoles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "JobRole Deleted!" });
        }

        private bool RoleExists(int id)
        {
            return _context.JobRoles.Any(e => e.Id == id);
        }
    }
}
