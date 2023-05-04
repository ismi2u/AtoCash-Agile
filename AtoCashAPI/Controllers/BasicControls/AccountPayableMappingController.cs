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
        [ActionName("AddEmployeesToBusinessUnit")]
        public async Task<ActionResult> AddEmployeesToBusinessUnit(AddEmployeesToBusinessUnit model)
        {

            int businessUnitId = model.BusinessUnitId;
            var businessUnit = _context.BusinessUnits.Find(businessUnitId);

            if (businessUnitId == 0 || businessUnit == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Business Unit is Invalid" });
            }

            //remove previous entries.
            List<AccountPayableMapping> BusinessUnits = await _context.AccountPayableMappings.Where(p => p.BusinessUnitId == businessUnitId).ToListAsync();
            _context.AccountPayableMappings.RemoveRange(BusinessUnits);


            //add new entries
            foreach (var empid in model.EmployeeIds)
            {
                AccountPayableMapping AccountPayableMapping = new();
                AccountPayableMapping.EmployeeId = empid;
                AccountPayableMapping.BusinessUnitId = businessUnitId;

                _context.AccountPayableMappings.Add(AccountPayableMapping);
            }

            await _context.SaveChangesAsync();
            return Ok(new RespStatus { Status = "Success", Message = "Business Unit assigned to Account Payable!" });

        }


        

        [HttpGet("{id}")]
        [ActionName("GetEmployeesByBusinessUnitId")]
        public async Task<ActionResult<List<GetEmployeesForBusinessUnit>>> GetEmployeesByBusinessUnitId(int id)
        {
            List<GetEmployeesForBusinessUnit> ListBusinessUnitpEmployees = new();

            /* Get Employee Having Account Payable Roles Only Start */
            var rolName = "AccPayable";
            var usersOfRole = await userManager.GetUsersInRoleAsync(rolName);
            

            foreach (ApplicationUser user in usersOfRole)
            {
                var emp = await _context.Employees.FindAsync(user.EmployeeId);
                
                if (emp != null)
                {
                    GetEmployeesForBusinessUnit BusinessUnitEmployee = new();

                    BusinessUnitEmployee.EmployeeId = emp.Id;
                    BusinessUnitEmployee.EmployeeName = _context.Employees.Find(emp.Id).GetFullName();
                    BusinessUnitEmployee.isAssigned = _context.AccountPayableMappings.Where(p => p.EmployeeId == emp.Id && p.BusinessUnitId == id).Any();
                    ListBusinessUnitpEmployees.Add(BusinessUnitEmployee);
                }
                 
            }          

            return Ok(ListBusinessUnitpEmployees);
            //var allEmployees = await _context.Employees.ToListAsync();

            /* Get Employee Having Account Payable Roles Only End */

        }

    }
}
