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
        [ActionName("AddEmployeesToCostCenter")]
        public async Task<ActionResult> AddEmployeesToCostCenter(AddEmployeesToCostCenter model)
        {

            int costCenterId = model.CostCenterId;
            var costCenterCode = _context.CostCenters.Find(costCenterId);

            if (costCenterCode == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Cost Center is Invalid" });
            }

            //remove previous entries.
            List<AccountPayableMapping> CostCenters = await _context.AccountPayableMappings.Where(p => p.CostCenterId == costCenterId).ToListAsync();
            _context.AccountPayableMappings.RemoveRange(CostCenters);


            //add new entries
            foreach (var empid in model.EmployeeIds)
            {
                AccountPayableMapping AccountPayableMapping = new();
                AccountPayableMapping.EmployeeId = empid;
                AccountPayableMapping.CostCenterId = costCenterId;

                _context.AccountPayableMappings.Add(AccountPayableMapping);
            }

            await _context.SaveChangesAsync();
            return Ok(new RespStatus { Status = "Success", Message = "Cost Center assigned to Account Payable!" });

        }


        

        [HttpGet("{id}")]
        [ActionName("GetEmployeesByCostCenterId")]
        public async Task<ActionResult<List<GetEmployeesForCostCenter>>> GetEmployeesByCostCenterId(int id)
        {
            List<GetEmployeesForCostCenter> ListCostCenterpEmployees = new();

            /* Get Employee Having Account Payable Roles Only Start */
            var rolName = "AccPayable";
            var usersOfRole = await userManager.GetUsersInRoleAsync(rolName);
            

            foreach (ApplicationUser user in usersOfRole)
            {
                var emp = await _context.Employees.FindAsync(user.EmployeeId);
                
                if (emp != null)
                {
                    GetEmployeesForCostCenter CostCenterEmployee = new();

                    CostCenterEmployee.EmployeeId = emp.Id;
                    CostCenterEmployee.EmployeeName = _context.Employees.Find(emp.Id).GetFullName();
                    CostCenterEmployee.isAssigned = _context.AccountPayableMappings.Where(p => p.EmployeeId == emp.Id && p.CostCenterId == id).Any();
                    ListCostCenterpEmployees.Add(CostCenterEmployee);
                }
                 
            }          

            return Ok(ListCostCenterpEmployees);
            //var allEmployees = await _context.Employees.ToListAsync();

            /* Get Employee Having Account Payable Roles Only End */

        }

    }
}
