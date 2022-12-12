using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtoCashAPI.Data;
using AtoCashAPI.Models;
using AtoCashAPI.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;

namespace AtoCashAPI.Controllers.BasicControls
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, Manager, User")]
    public class EmployeesController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public EmployeesController(AtoCashDbContext context)
        {
            _context = context;
        }

        // GET: api/Employees
        [HttpGet]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, Manager, User")]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetEmployees()
        {
            List<EmployeeDTO> ListEmployeeDTO = new();

            var employees = await _context.Employees.ToListAsync();

            foreach (Employee employee in employees)
            {
                Bank? empBank = _context.Banks.Find(employee.BankId ?? 0);

                EmployeeDTO employeeDTO = new();

                employeeDTO.Id = employee.Id;
                employeeDTO.FirstName = employee.FirstName;
                employeeDTO.MiddleName = employee.MiddleName;
                employeeDTO.LastName = employee.LastName;
                employeeDTO.EmpCode = employee.EmpCode;
                employeeDTO.IBAN = employee.IBAN;
                employeeDTO.BankId = employee.BankId;
                employeeDTO.BankName = empBank == null ? "" : empBank.BankName;
                employeeDTO.BankAccount = employee.BankAccount;
                employeeDTO.BankCardNo = employee.BankCardNo;
                employeeDTO.NationalID = employee.NationalID;
                employeeDTO.PassportNo = employee.PassportNo;
                employeeDTO.TaxNumber = employee.TaxNumber;
                employeeDTO.Nationality = employee.Nationality;
                employeeDTO.DOB = employee.DOB;
                employeeDTO.DOJ = employee.DOJ;
                employeeDTO.Gender = employee.Gender;
                employeeDTO.Email = employee.Email;
                employeeDTO.MobileNumber = employee.MobileNumber;
                employeeDTO.EmploymentTypeId = employee.EmploymentTypeId;
                employeeDTO.CurrencyTypeId = employee.CurrencyTypeId;
                employeeDTO.StatusTypeId = employee.StatusTypeId;
                //employeeDTO.DepartmentId = employee.DepartmentId;
                //employeeDTO.JobRoleId = employee.JobRoleId;
                //employeeDTO.ApprovalGroupId = (int)employee.ApprovalGroupId;
                //employeeDTO.BusinessAreaApprovalGroupId = employee.BusinessAreaApprovalGroupId ?? 0;
                //employeeDTO.BusinessAreaRoleId = employee.BusinessAreaRoleId ?? 0;
                //employeeDTO.BusinessAreaId = employee.BusinessAreaId ?? 0;



                ListEmployeeDTO.Add(employeeDTO);

            }

            return ListEmployeeDTO;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, Manager, User")]
        public async Task<ActionResult<EmployeeDTO>> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);


            if (employee == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id invalid!" });
            }

            EmployeeDTO employeeDTO = new();

            Bank? empBank = _context.Banks.Find(employee.BankId ?? 0);

            employeeDTO.Id = employee.Id;
            employeeDTO.FirstName = employee.FirstName;
            employeeDTO.MiddleName = employee.MiddleName;
            employeeDTO.LastName = employee.LastName;
            employeeDTO.EmpCode = employee.EmpCode;
            employeeDTO.IBAN = employee.IBAN;
            employeeDTO.BankId = employee.BankId;
            employeeDTO.BankName = empBank == null ? "" : empBank.BankName;
            employeeDTO.BankAccount = employee.BankAccount;
            employeeDTO.BankCardNo = employee.BankCardNo;
            employeeDTO.NationalID = employee.NationalID;
            employeeDTO.PassportNo = employee.PassportNo;
            employeeDTO.TaxNumber = employee.TaxNumber;
            employeeDTO.Nationality = employee.Nationality;
            employeeDTO.DOB = employee.DOB;
            employeeDTO.DOJ = employee.DOJ;
            employeeDTO.Gender = employee.Gender;
            employeeDTO.Email = employee.Email;
            employeeDTO.MobileNumber = employee.MobileNumber;
            employeeDTO.EmploymentTypeId = employee.EmploymentTypeId;
            employeeDTO.CurrencyTypeId = employee.CurrencyTypeId;
            employeeDTO.StatusTypeId = employee.StatusTypeId;
            //employeeDTO.DepartmentId = employee.DepartmentId;
            //employeeDTO.JobRoleId = employee.JobRoleId;
            //employeeDTO.ApprovalGroupId = (int)employee.ApprovalGroupId;
            //employeeDTO.BusinessAreaApprovalGroupId = employee.BusinessAreaApprovalGroupId ?? 0;
            //employeeDTO.BusinessAreaRoleId = employee.BusinessAreaRoleId ?? 0;
            //employeeDTO.BusinessAreaId = employee.BusinessAreaId ?? 0;


            return employeeDTO;
        }
        [HttpGet]
        [ActionName("EmployeesForDropdown")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, Manager, User")]
        public async Task<ActionResult<IEnumerable<EmployeeVM>>> GetEmployeesForDropDown()
        {
            List<EmployeeVM> ListEmployeeVM = new();

            var employees = await _context.Employees.ToListAsync();

            foreach (Employee employee in employees)
            {
                var emp = _context.Employees.Find(employee.Id);
                EmployeeVM employeeVM = new EmployeeVM();
                if (emp != null)
                {
                    employeeVM.Id = employee.Id;
                    employeeVM.Email = employee.Email;
                    employeeVM.FullName = employee.EmpCode + ":" + emp.GetFullName();
                }
                ListEmployeeVM.Add(employeeVM);
            }

            return ListEmployeeVM;

        }


        [HttpGet("{id}")]
        [ActionName("GetReporteesUnderManager")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, Manager, User")]
        public async Task<ActionResult<IEnumerable<EmployeeVM>>> GetReporteesUnderManager(int id)
        {

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id invalid!" });
            }

            // all employees who report under the manager
            //int mgrDeptId = employee.DepartmentId;
            //List<Employee> mgrReportees = _context.Employees.Where(e => e.DepartmentId == mgrDeptId && e.Id != id).ToList();

            List<EmployeeVM> employeeVMs = new();
            //foreach (Employee reportee in mgrReportees)
            //{
            //    EmployeeVM employeeVM = new();
            //    employeeVM.Id = reportee.Id;
            //    employeeVM.FullName = reportee.GetFullName();

            //    employeeVMs.Add(employeeVM);
            //}

            return Ok(employeeVMs);
        }


        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutEmployee(int id, EmployeeDTO employeeDto)
        {
            if (id != employeeDto.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id is invalid" });
            }

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee object is null" });
            }
            else
            {

                try
                {
                    int? _testempId = _context.Employees.Where(e => e.MobileNumber == employeeDto.MobileNumber || e.EmpCode == employeeDto.EmpCode || e.Email == employeeDto.Email).Select(x => x.Id).FirstOrDefault();

                    if (employeeDto.Id != _testempId)
                    {
                        return Conflict(new RespStatus { Status = "Failure", Message = "Unique EmpCode/Mobile/Email required" });
                    }

                    employee.FirstName = employeeDto.FirstName;
                    employee.MiddleName = employeeDto.MiddleName;
                    employee.LastName = employeeDto.LastName;
                    employee.EmpCode = employeeDto.EmpCode;
                    employee.BankAccount = employeeDto.BankAccount;
                    employee.IBAN = employeeDto.IBAN;
                    employee.BankId = employeeDto.BankId;
                    employee.BankCardNo = employeeDto.BankCardNo;
                    employee.NationalID = employeeDto.NationalID;
                    employee.PassportNo = employeeDto.PassportNo;
                    employee.TaxNumber = employeeDto.TaxNumber;
                    employee.Nationality = employeeDto.Nationality;
                    employee.DOB = employeeDto.DOB;
                    employee.DOJ = employeeDto.DOJ;
                    employee.Gender = employeeDto.Gender;
                    employee.Email = employeeDto.Email;
                    employee.MobileNumber = employeeDto.MobileNumber;
                    employee.EmploymentTypeId = employeeDto.EmploymentTypeId;
                    employee.CurrencyTypeId = employeeDto.CurrencyTypeId;
                    employee.StatusTypeId = employeeDto.StatusTypeId;
                    //employee.ApprovalGroupId = employeeDto.ApprovalGroupId;
                    //employee.DepartmentId = employeeDto.DepartmentId;
                    //employee.BusinessAreaApprovalGroupId = employeeDto.BusinessAreaApprovalGroupId;
                    //employee.BusinessAreaRoleId = employeeDto.BusinessAreaRoleId;
                    //employee.BusinessAreaId = employeeDto.BusinessAreaId;


                    //if (employee.RoleId != employeeDto.RoleId)
                    //{

                    //    double oldAmt = _context.JobRoles.Find(employee.RoleId).MaxPettyCashAllowed;
                    //    double newAmt = _context.JobRoles.Find(employeeDto.RoleId).MaxPettyCashAllowed;
                    //    EmpCurrentPettyCashBalance empCurrentPettyCashBalance = _context.EmpCurrentPettyCashBalances.Where(e => e.EmployeeId == employee.Id).FirstOrDefault();
                    //    double empCurBal = empCurrentPettyCashBalance.CurBalance;

                    //    //update the roleId to new RoleId
                    //    employee.RoleId = employeeDto.RoleId;

                    //    double usedAmount = oldAmt - empCurrentPettyCashBalance.CurBalance;
                    //    double NewUpdatedLimit = newAmt - usedAmount;

                    //    empCurrentPettyCashBalance.CurBalance = NewUpdatedLimit;
                    //    _context.EmpCurrentPettyCashBalances.Update(empCurrentPettyCashBalance);
                    //}

                    _context.Employees.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Conflict(new RespStatus { Status = "Failure", Message = "Employee cannot be updated!" });

                }
            }



            return Ok(new RespStatus { Status = "Success", Message = "Employee Records Updated!" });
        }

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<Employee>> PostEmployee(EmployeeDTO employeeDto)
        {

            //var emplye = _context.Employees.Where(e => e.FirstName == employeeDto.FirstName && e.MiddleName == employeeDto.MiddleName && e.LastName == employeeDto.LastName).FirstOrDefault();

            var emp = _context.Employees.Where(e => e.Email == employeeDto.Email || e.EmpCode == employeeDto.EmpCode || e.MobileNumber == employeeDto.MobileNumber).FirstOrDefault();

            if (emp != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Unique EmpCode/Mobile/Email required" });
            }

            try
            {
                Employee employee = new Employee();

                employee.FirstName = employeeDto.FirstName;
                employee.MiddleName = employeeDto.MiddleName;
                employee.LastName = employeeDto.LastName;
                employee.EmpCode = employeeDto.EmpCode;
                employee.IBAN = employeeDto.IBAN;
                employee.BankId = employeeDto.BankId;
                employee.BankAccount = employeeDto.BankAccount;
                employee.BankCardNo = employeeDto.BankCardNo;
                employee.NationalID = employeeDto.NationalID;
                employee.PassportNo = employeeDto.PassportNo;
                employee.TaxNumber = employeeDto.TaxNumber;
                employee.Nationality = employeeDto.Nationality;
                employee.DOB = employeeDto.DOB;
                employee.DOJ = employeeDto.DOJ;
                employee.Gender = employeeDto.Gender;
                employee.Email = employeeDto.Email;
                employee.MobileNumber = employeeDto.MobileNumber;
                employee.EmploymentTypeId = employeeDto.EmploymentTypeId;
                employee.CurrencyTypeId = employeeDto.CurrencyTypeId;
                employee.StatusTypeId = employeeDto.StatusTypeId;

                //employee.BusinessAreaApprovalGroupId = employeeDto.BusinessAreaApprovalGroupId;
                //employee.BusinessAreaRoleId = employeeDto.BusinessAreaRoleId;
                //employee.BusinessAreaId = employeeDto.BusinessAreaId;
                //employee.DepartmentId = employeeDto.DepartmentId;
                //employee.JobRoleId = employeeDto.JobRoleId;
                //employee.ApprovalGroupId = employeeDto.ApprovalGroupId;
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();


                //Add PettyCash Balance
                _context.EmpCurrentPettyCashBalances.Add(new EmpCurrentPettyCashBalance()
                {

                    EmployeeId = employee.Id,
                    MaxPettyCashLimit = 0,
                    CurBalance = 0,
                    CashOnHand = 0,
                    UpdatedOn = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Doesn't Exist!" });
            }

            return Ok(new RespStatus { Status = "Success", Message = "New Employee Created!" });
        }

        // DELETE: api/Employees/5
        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id invalid!" });
            }

            if (_context.Users.Where(u => u.EmployeeId == id).Any())
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee is a User, cant delete!" });
            }


            bool blnUsedInTravelReq = _context.TravelApprovalRequests.Where(t => t.EmployeeId == id).Any();
            bool blnUsedInCashAdvReq = _context.PettyCashRequests.Where(t => t.EmployeeId == id).Any();
            bool blnUsedInExpeReimReq = _context.ExpenseReimburseRequests.Where(t => t.EmployeeId == id).Any();

            if (blnUsedInTravelReq || blnUsedInCashAdvReq || blnUsedInExpeReimReq)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee in Use, Cant delete!" });
            }

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {
                _context.Employees.Remove(employee);
                var empPettyCashBal = _context.EmpCurrentPettyCashBalances.Where(e => e.EmployeeId == id).FirstOrDefault();
                _context.EmpCurrentPettyCashBalances.Remove(empPettyCashBal);
                await _context.SaveChangesAsync();

                await AtoCashDbContextTransaction.CommitAsync();
            }

            return Ok(new RespStatus { Status = "Success", Message = "Employee Deleted!" });

        }

      
    }
}
