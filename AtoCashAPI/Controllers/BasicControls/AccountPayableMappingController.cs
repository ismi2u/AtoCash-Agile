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
using Microsoft.AspNetCore.Identity;

namespace AtoCashAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User")]
    public class AccountPayableMappingController : ControllerBase
    {
        private readonly AtoCashDbContext _context;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AccountPayableMappingController(AtoCashDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpPost]
        [ActionName("AddEmployeesToApprovalGroup")]
        public async Task<ActionResult> AddEmployeesToApprovalGroup(AddEmployeesToApprovalGroup model)
        {

            int appgrpId = model.ApprovalGroupId;
            var approvalGroup = _context.ApprovalGroups.Find(appgrpId);

            if (appgrpId == 0 || approvalGroup == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Approval Group is Invalid" });
            }

            //remove previous entries.
            List<AccountPayableMapping> AppGroupItems = await _context.AccountPayableMappings.Where(p => p.ApprovalGroupId == appgrpId).ToListAsync();
            _context.AccountPayableMappings.RemoveRange(AppGroupItems);


            //add new entries
            foreach (var empid in model.EmployeeIds)
            {
                AccountPayableMapping AccountPayableMapping = new();
                AccountPayableMapping.EmployeeId = empid;
                AccountPayableMapping.ApprovalGroupId = appgrpId;

                _context.AccountPayableMappings.Add(AccountPayableMapping);
            }

            await _context.SaveChangesAsync();
            return Ok(new RespStatus { Status = "Success", Message = "Approval Group assigned to Account Payable!" });

        }


        

        [HttpGet("{id}")]
        [ActionName("GetEmployeesByApprovalGroupId")]
        public async Task<ActionResult<List<GetEmployeesForApprovalGroup>>> GetEmployeesByAccountPayableMappingId(int id)
        {
            List<GetEmployeesForApprovalGroup> ListApprovalGroupEmployees = new();

            /* Get Employee Having Account Payable Roles Only Start */
            var rolName = "AccPayable";
            var usersOfRole = await userManager.GetUsersInRoleAsync(rolName);
            

            foreach (ApplicationUser user in usersOfRole)
            {
                var emp = await _context.Employees.FindAsync(user.EmployeeId);
                
                if (emp != null)
                {
                    GetEmployeesForApprovalGroup AppGrpEmployee = new(); 
                    
                    AppGrpEmployee.EmployeeId = emp.Id;
                    AppGrpEmployee.EmployeeName = _context.Employees.Find(emp.Id).GetFullName();
                    AppGrpEmployee.isAssigned = _context.AccountPayableMappings.Where(p => p.EmployeeId == emp.Id && p.ApprovalGroupId == id).Any();
                    ListApprovalGroupEmployees.Add(AppGrpEmployee);
                }
                 
            }          

            return Ok(ListApprovalGroupEmployees);
            //var allEmployees = await _context.Employees.ToListAsync();

            /* Get Employee Having Account Payable Roles Only End */

        }

    }
}
