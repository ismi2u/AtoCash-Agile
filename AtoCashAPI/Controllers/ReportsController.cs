using AtoCashAPI.Authentication;
using AtoCashAPI.Data;
using AtoCashAPI.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using LinqKit;
using System.Data.Entity;

namespace AtoCashAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User, AccPayable")]
    public class ReportsController : ControllerBase
    {
        private readonly AtoCashDbContext _context;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public ReportsController(AtoCashDbContext context, IWebHostEnvironment hostEnv, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            //var user = System.Threading.Thread.CurrentPrincipal;
            //var TheUser = User.Identity.IsAuthenticated ? UserRepository.GetUser(user.Identity.Name) : null;
            _context = context;
            hostingEnvironment = hostEnv;
            this.roleManager = roleManager;
            this.userManager = userManager;
            //Get Logged in User's EmpId.
            //var   LoggedInEmpid = User.Identities.First().Claims.ToList().Where(x => x.Type == "EmployeeId").Select(c => c.Value);
        }



        [HttpPost]
        [ActionName("GetUsersByRoleIdReport")]
        public async Task<IActionResult> GetUsersByRoleIdReport(RoleToUserSearch searchmodel)
        {
            List<UserByRole> ListUserByRole = new();

            if (!string.IsNullOrEmpty(searchmodel.RoleId))
            {
                string rolName = roleManager.Roles.Where(r => r.Id == searchmodel.RoleId).FirstOrDefault().Name;

                //  List<string> UserIds =  context.UserRoles.Where(r => r.RoleId == id).Select(b => b.UserId).Distinct().ToList();

                var AllRoles = roleManager.Roles.ToList();

                var usersOfRole = await userManager.GetUsersInRoleAsync(rolName);

                foreach (ApplicationUser user in usersOfRole)
                {
                    UserByRole userByRole = new();

                    List<string> strJobRolesInString = new List<string>();
                    var emp = await _context.Employees.FindAsync(user.EmployeeId);

                    if (emp != null)
                    {
                        userByRole.UserId = user.Id;
                        userByRole.Id = emp.Id;
                        userByRole.UserFullName = emp.GetFullName();
                        userByRole.Email = emp.Email;
                        userByRole.EmpCode = emp.EmpCode;
                        userByRole.DOB = emp.DOB;
                        userByRole.DOJ = emp.DOJ;
                        userByRole.Gender = emp.Gender;
                        userByRole.JobRoles = getJobRolesByEmployeeId(user.EmployeeId ?? 0);
                        userByRole.MobileNumber = emp.MobileNumber;
                        userByRole.AccessRole = rolName;
                        userByRole.StatusType = _context.StatusTypes.Find(emp.StatusTypeId).Status;

                    }



                    ListUserByRole.Add(userByRole);
                }
            }
            else
            {

                var users = userManager.Users.ToList();
                foreach (var user in users)
                {
                    UserByRole userByRole = new();
                    var roles = await userManager.GetRolesAsync(user);


                    if (user.EmployeeId == 0)
                    {
                        continue;
                    }
                    var emp = await _context.Employees.FindAsync(user.EmployeeId);

                    userByRole.UserId = user.Id;
                    userByRole.Id = emp.Id;
                    userByRole.UserFullName = emp.GetFullName();
                    userByRole.Email = emp.Email;
                    userByRole.EmpCode = emp.EmpCode;
                    userByRole.DOB = emp.DOB;
                    userByRole.DOJ = emp.DOJ;
                    userByRole.Gender = emp.Gender;
                    userByRole.MobileNumber = emp.MobileNumber;
                    userByRole.BusinessUnits = getBusinessUnitsByEmployeeId(user.EmployeeId ?? 0);
                    userByRole.JobRoles = getJobRolesByEmployeeId(user.EmployeeId ?? 0);
                    userByRole.StatusType = _context.StatusTypes.Find(emp.StatusTypeId).Status;
                    foreach (var r in roles)
                    {

                        if (userByRole.AccessRole == null)
                        {
                            userByRole.AccessRole = "";
                        }
                        if (userByRole.AccessRole == "")
                        {
                            userByRole.AccessRole = r;
                        }
                        else
                        {
                            userByRole.AccessRole = userByRole.AccessRole + "," + r;
                        }
                    }
                    ListUserByRole.Add(userByRole);
                }
            }


            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[13]
                {
                    //new DataColumn("Id", typeof(int)),
                    new DataColumn("UserId", typeof(string)),
                    new DataColumn("EmployeeId", typeof(int)),
                    new DataColumn("UserFullName", typeof(string)),
                    new DataColumn("Email",typeof(string)),
                    new DataColumn("EmpCode",typeof(string)),
                    new DataColumn("DOB",typeof(string)),
                    new DataColumn("DOJ", typeof(string)),
                    new DataColumn("Gender",typeof(string)),
                    new DataColumn("MobileNumber",typeof(string)),
                    new DataColumn("BusinessUnit(s)",typeof(string)),
                    new DataColumn("JobRole(s)", typeof(string)),
                    new DataColumn("StatusType", typeof(string)),
                    new DataColumn("AccessRole", typeof(string))

                });

            foreach (var usr in ListUserByRole)
            {
                dt.Rows.Add(
                    usr.UserId,
                    usr.Id,
                    usr.UserFullName,
                    usr.Email,
                    usr.EmpCode,
                    usr.DOB,
                    usr.DOJ,
                    usr.Gender,
                    usr.MobileNumber,
                    usr.BusinessUnit,
                    usr.JobRoles,
                    usr.StatusType,
                    usr.AccessRole
                    );
            }
            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook
            List<string> docUrls = new();
            var docUrl = GetExcel("GetUsersByRoleId", dt);

            docUrls.Add(docUrl);

            return Ok(docUrls);
        }



        [HttpPost]
        [ActionName("GetUsersByRoleId")]
        public async Task<IActionResult> GetUsersByRoleId(RoleToUserSearch searchmodel)
        {
            List<UserByRole> ListUserByRole = new();

            if (!string.IsNullOrEmpty(searchmodel.RoleId))
            {
                string rolName = roleManager.Roles.Where(r => r.Id == searchmodel.RoleId).FirstOrDefault().Name;

                //  List<string> UserIds =  context.UserRoles.Where(r => r.RoleId == id).Select(b => b.UserId).Distinct().ToList();

                var AllRoles = roleManager.Roles.ToList();

                var usersOfRole = await userManager.GetUsersInRoleAsync(rolName);

                foreach (ApplicationUser user in usersOfRole)
                {
                    UserByRole userByRole = new();

                    var emp = await _context.Employees.FindAsync(user.EmployeeId);
                    userByRole.UserId = user.Id;
                    userByRole.Id = emp.Id;
                    userByRole.UserFullName = emp.GetFullName();
                    userByRole.Email = emp.Email;
                    userByRole.EmpCode = emp.EmpCode;
                    userByRole.DOB = emp.DOB;
                    userByRole.DOJ = emp.DOJ;
                    userByRole.Gender = emp.Gender;
                    userByRole.MobileNumber = emp.MobileNumber;
                    userByRole.JobRoles = getJobRolesByEmployeeId(user.EmployeeId ?? 0);
                    userByRole.AccessRole = rolName;
                    userByRole.StatusType = _context.StatusTypes.Find(emp.StatusTypeId).Status;

                    ListUserByRole.Add(userByRole);
                }
            }
            else
            {

                var users = userManager.Users.ToList();
                foreach (var user in users)
                {
                    UserByRole userByRole = new();
                    var roles = await userManager.GetRolesAsync(user);


                    if (user.EmployeeId == 0)
                    {
                        continue;
                    }
                    var emp = await _context.Employees.FindAsync(user.EmployeeId);

                    userByRole.UserId = user.Id;
                    userByRole.Id = emp.Id;
                    userByRole.UserFullName = emp.GetFullName();
                    userByRole.Email = emp.Email;
                    userByRole.EmpCode = emp.EmpCode;
                    userByRole.DOB = emp.DOB;
                    userByRole.DOJ = emp.DOJ;
                    userByRole.Gender = emp.Gender;
                    userByRole.MobileNumber = emp.MobileNumber;
                    userByRole.JobRoles = getJobRolesByEmployeeId(user.EmployeeId ?? 0);
                    userByRole.StatusType = _context.StatusTypes.Find(emp.StatusTypeId).Status;
                    foreach (var r in roles)
                    {

                        if (userByRole.AccessRole == null)
                        {
                            userByRole.AccessRole = "";
                        }
                        if (userByRole.AccessRole == "")
                        {
                            userByRole.AccessRole = r;
                        }
                        else
                        {
                            userByRole.AccessRole = userByRole.AccessRole + "," + r;
                        }
                    }
                    ListUserByRole.Add(userByRole);
                }
            }

            return Ok(ListUserByRole);
        }

        [HttpPost]
        [ActionName("GetEmployeeExtendedInfoData")]
        public async Task<IActionResult> GetEmployeeExtendedInfoData(EmpExtInfoSearchModel searchmodel)
        {

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<EmployeeExtendedInfo>();

            if (searchmodel.EmployeeId != 0 && searchmodel.EmployeeId != null)
                predicate = predicate.And(x => x.Id == searchmodel.EmployeeId);
            if (searchmodel.BusinessTypeId != 0 && searchmodel.BusinessTypeId != null)
                predicate = predicate.And(x => x.BusinessTypeId == searchmodel.BusinessTypeId);
            if (searchmodel.BusinessUnitId != 0 && searchmodel.BusinessUnitId != null)
                predicate = predicate.And(x => x.BusinessUnitId == searchmodel.BusinessUnitId);
            if (searchmodel.JobRoleId != 0 && searchmodel.JobRoleId != null)
                predicate = predicate.And(x => x.JobRoleId == searchmodel.JobRoleId);
            if (searchmodel.ApprovalGroupId != 0 && searchmodel.ApprovalGroupId != null)
                predicate = predicate.And(x => x.ApprovalGroupId == searchmodel.ApprovalGroupId);
            if (searchmodel.StatusTypeId != 0 && searchmodel.StatusTypeId != null)
                predicate = predicate.And(x => x.StatusTypeId == searchmodel.StatusTypeId);



            List<EmployeeExtendedInfo> result = new();


            if (predicate.IsStarted)
            {
                result = _context.EmployeeExtendedInfos.Where(predicate).ToList();
            }
            else
            {
                result = _context.EmployeeExtendedInfos.ToList();
            }


            List<EmployeeExtendedInfoDTO> ListEmployeeExtendedInfoDTOs = new();


            await Task.Run(() =>
            {
                foreach (EmployeeExtendedInfo employeeExtendedInfo in result)
                {
                    EmployeeExtendedInfoDTO empExtendedInfoDTO = new();

                    empExtendedInfoDTO.Id = employeeExtendedInfo.Id;
                    empExtendedInfoDTO.EmployeeId = employeeExtendedInfo.EmployeeId;
                    empExtendedInfoDTO.Employee = _context.Employees.Find(employeeExtendedInfo.EmployeeId).GetFullName();
                    empExtendedInfoDTO.BusinessTypeId = employeeExtendedInfo.BusinessTypeId;
                    empExtendedInfoDTO.BusinessType = _context.BusinessTypes.Find(employeeExtendedInfo.BusinessTypeId).BusinessTypeName;
                    empExtendedInfoDTO.BusinessUnitId = employeeExtendedInfo.BusinessUnitId;
                    empExtendedInfoDTO.BusinessUnit = _context.BusinessUnits.Find(employeeExtendedInfo.BusinessUnitId).GetBusinessUnitName();
                    empExtendedInfoDTO.JobRoleId = employeeExtendedInfo.JobRoleId;
                    empExtendedInfoDTO.JobRole = _context.JobRoles.Find(employeeExtendedInfo.JobRoleId).GetJobRole();
                    empExtendedInfoDTO.ApprovalGroupId = employeeExtendedInfo.ApprovalGroupId;
                    empExtendedInfoDTO.ApprovalGroup = _context.ApprovalGroups.Find(employeeExtendedInfo.ApprovalGroupId).ApprovalGroupCode;
                    empExtendedInfoDTO.StatusTypeId = employeeExtendedInfo.StatusTypeId;
                    empExtendedInfoDTO.StatusType = _context.StatusTypes.Find(employeeExtendedInfo.StatusTypeId).Status;

                    ListEmployeeExtendedInfoDTOs.Add(empExtendedInfoDTO);

                }
            });


            return Ok(ListEmployeeExtendedInfoDTOs);
        }


        [HttpPost]
        [ActionName("GetEmployeeExtendedInfoReport")]
        public async Task<IActionResult> GetEmployeeExtendedInfoReport(EmpExtInfoSearchModel searchmodel)
        {

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<EmployeeExtendedInfo>();

            if (searchmodel.EmployeeId != 0 && searchmodel.EmployeeId != null)
                predicate = predicate.And(x => x.Id == searchmodel.EmployeeId);
            if (searchmodel.BusinessTypeId != 0 && searchmodel.BusinessTypeId != null)
                predicate = predicate.And(x => x.BusinessTypeId == searchmodel.BusinessTypeId);
            if (searchmodel.BusinessUnitId != 0 && searchmodel.BusinessUnitId != null)
                predicate = predicate.And(x => x.BusinessUnitId == searchmodel.BusinessUnitId);
            if (searchmodel.JobRoleId != 0 && searchmodel.JobRoleId != null)
                predicate = predicate.And(x => x.JobRoleId == searchmodel.JobRoleId);
            if (searchmodel.ApprovalGroupId != 0 && searchmodel.ApprovalGroupId != null)
                predicate = predicate.And(x => x.ApprovalGroupId == searchmodel.ApprovalGroupId);
            if (searchmodel.StatusTypeId != 0 && searchmodel.StatusTypeId != null)
                predicate = predicate.And(x => x.StatusTypeId == searchmodel.StatusTypeId);



            List<EmployeeExtendedInfo> result = new();


            if (predicate.IsStarted)
            {
                result = _context.EmployeeExtendedInfos.Where(predicate).ToList();
            }
            else
            {
                result = _context.EmployeeExtendedInfos.ToList();
            }


            List<EmployeeExtendedInfoDTO> ListEmployeeExtendedInfoDTOs = new();


            await Task.Run(() =>
            {
                foreach (EmployeeExtendedInfo employeeExtendedInfo in result)
                {
                    EmployeeExtendedInfoDTO empExtendedInfoDTO = new();

                    empExtendedInfoDTO.Id = employeeExtendedInfo.Id;
                    empExtendedInfoDTO.EmployeeId = employeeExtendedInfo.EmployeeId;
                    empExtendedInfoDTO.Employee = _context.Employees.Find(employeeExtendedInfo.EmployeeId).GetFullName();
                    empExtendedInfoDTO.BusinessTypeId = employeeExtendedInfo.BusinessTypeId;
                    empExtendedInfoDTO.BusinessType = _context.BusinessTypes.Find(employeeExtendedInfo.BusinessTypeId).BusinessTypeName;
                    empExtendedInfoDTO.BusinessUnitId = employeeExtendedInfo.BusinessUnitId;
                    empExtendedInfoDTO.BusinessUnit = _context.BusinessUnits.Find(employeeExtendedInfo.BusinessUnitId).GetBusinessUnitName();
                    empExtendedInfoDTO.JobRoleId = employeeExtendedInfo.JobRoleId;
                    empExtendedInfoDTO.JobRole = _context.JobRoles.Find(employeeExtendedInfo.JobRoleId).GetJobRole();
                    empExtendedInfoDTO.ApprovalGroupId = employeeExtendedInfo.ApprovalGroupId;
                    empExtendedInfoDTO.ApprovalGroup = _context.ApprovalGroups.Find(employeeExtendedInfo.ApprovalGroupId).ApprovalGroupCode;
                    empExtendedInfoDTO.StatusTypeId = employeeExtendedInfo.StatusTypeId;
                    empExtendedInfoDTO.StatusType = _context.StatusTypes.Find(employeeExtendedInfo.StatusTypeId).Status;

                    ListEmployeeExtendedInfoDTOs.Add(empExtendedInfoDTO);

                }
            });


            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[11]
                {
                    //new DataColumn("Id", typeof(int)),
                    new DataColumn("EmployeeId", typeof(int)),
                    new DataColumn("EmployeeFullName", typeof(string)),
                    new DataColumn("BusinessTypeId",typeof(int)),
                    new DataColumn("BusinessType",typeof(string)),
                    new DataColumn("BusinessUnitId",typeof(int)),
                    new DataColumn("BusinessUnit",typeof(string)),
                    new DataColumn("JobRoleId",typeof(int)),
                    new DataColumn("JobRole",typeof(string)),
                    new DataColumn("ApprovalGroupId",typeof(int)),
                    new DataColumn("ApprovalGroup", typeof(string)),
                    new DataColumn("StatusType", typeof(string))
                });

            foreach (var empExtInfo in ListEmployeeExtendedInfoDTOs)
            {
                dt.Rows.Add(
                    //empExtInfo.Id,
                    empExtInfo.EmployeeId,
                    empExtInfo.Employee,
                    empExtInfo.BusinessTypeId,
                    empExtInfo.BusinessType,
                    empExtInfo.BusinessUnitId,
                    empExtInfo.BusinessUnit,
                    empExtInfo.JobRoleId,
                    empExtInfo.JobRole,
                    empExtInfo.ApprovalGroupId,
                    empExtInfo.ApprovalGroup,
                    empExtInfo.StatusType
                    );
            }

            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook

            List<string> docUrls = new();
            var docUrl = GetExcel("GetEmpExtendedInfo", dt);

            docUrls.Add(docUrl);

            return Ok(docUrls);


        }





        [HttpPost]
        [ActionName("GetEmployeesReport")]
        public async Task<IActionResult> GetEmployeesReport(EmployeeSearchModel searchModel)
        {

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<Employee>();


            if (searchModel.EmployeeId != 0 && searchModel.EmployeeId != null)
                predicate = predicate.And(x => x.Id == searchModel.EmployeeId);
            if (!string.IsNullOrEmpty(searchModel.EmployeeName))
                predicate = predicate.And(x => x.GetFullName().Contains(searchModel.EmployeeName));
            if (!string.IsNullOrEmpty(searchModel.EmpCode))
                predicate = predicate.And(x => x.EmpCode.Contains(searchModel.EmpCode));
            if (!string.IsNullOrEmpty(searchModel.Nationality))
                predicate = predicate.And(x => x.Nationality.Contains(searchModel.Nationality));
            if (!string.IsNullOrEmpty(searchModel.Gender))
                predicate = predicate.And(x => x.Gender.Contains(searchModel.Gender));
            if (searchModel.EmploymentTypeId != 0 && searchModel.EmploymentTypeId != null)
                predicate = predicate.And(x => x.EmploymentTypeId == searchModel.EmploymentTypeId);
            //if (searchModel.BusinessUnitId != 0 && searchModel.BusinessUnitId != null)
            //    predicate = predicate.And(x => x.business == searchModel.BusinessUnitId);
            //if (searchModel.JobRoleId != 0 && searchModel.JobRoleId != null)
            //    predicate = predicate.And(x => x.RoleId == searchModel.JobRoleId);
            //if (searchModel.ApprovalGroupId != 0 && searchModel.ApprovalGroupId != null)
            //    predicate = predicate.And(x => x.ApprovalGroupId == searchModel.ApprovalGroupId);
            if (searchModel.StatusTypeId != 0 && searchModel.StatusTypeId != null)
                predicate = predicate.And(x => x.StatusTypeId == searchModel.StatusTypeId);



            List<Employee> result = new();
            if (predicate.IsStarted)
            {
                result = _context.Employees.Where(predicate).ToList();
            }
            else
            {
                result = _context.Employees.ToList();
            }



            List<EmployeeDTO> ListEmployeeDto = new();


            await Task.Run(() =>
            {
                foreach (Employee employee in result)
                {
                    EmployeeDTO employeeDTO = new();

                    employeeDTO.Id = employee.Id;
                    employeeDTO.FullName = employee.GetFullName();
                    employeeDTO.EmpCode = employee.EmpCode;
                    employeeDTO.BankAccount = employee.BankAccount;
                    employeeDTO.BankCardNo = employee.BankCardNo;
                    employeeDTO.PassportNo = employee.PassportNo;
                    employeeDTO.TaxNumber = employee.TaxNumber;
                    employeeDTO.Nationality = employee.Nationality;
                    employeeDTO.DOB = employee.DOB;
                    employeeDTO.DOJ = employee.DOJ;
                    employeeDTO.Gender = employee.Gender;
                    employeeDTO.Email = employee.Email;
                    employeeDTO.MobileNumber = employee.MobileNumber;
                    employeeDTO.EmploymentType = employee.EmploymentTypeId != 0 ? _context.EmploymentTypes.Find(employee.EmploymentTypeId).EmpJobTypeCode + ":" + _context.EmploymentTypes.Find(employee.EmploymentTypeId).EmpJobTypeDesc : string.Empty;
                    employeeDTO.BusinessUnits = getBusinessUnitsByEmployeeId(employee.Id);
                    employeeDTO.JobRoles = getJobRolesByEmployeeId(employee.Id);
                    employeeDTO.ApprovalGroups = getApprovalGroupsByEmployeeId(employee.Id);
                    employeeDTO.StatusType = employee.StatusTypeId != 0 ? _context.StatusTypes.Find(employee.StatusTypeId).Status : string.Empty;

                    ListEmployeeDto.Add(employeeDTO);
                }
            });

            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[18]
                {
                    //new DataColumn("Id", typeof(int)),
                    new DataColumn("EmployeeId", typeof(int)),
                    new DataColumn("EmployeeFullName", typeof(string)),
                    new DataColumn("EmpCode",typeof(string)),
                    new DataColumn("BankAccount",typeof(string)),
                    new DataColumn("BankCardNo",typeof(string)),
                    new DataColumn("PassportNo",typeof(string)),
                    new DataColumn("TaxNumber",typeof(string)),
                    new DataColumn("Nationality",typeof(string)),
                    new DataColumn("DOB",typeof(string)),
                    new DataColumn("DOJ", typeof(string)),
                    new DataColumn("Gender",typeof(string)),
                    new DataColumn("Email",typeof(string)),
                    new DataColumn("MobileNumber",typeof(string)),
                    new DataColumn("EmploymentType",typeof(string)),
                    new DataColumn("BusinessUnit(s)",typeof(string)),
                    new DataColumn("JobRole(s)", typeof(string)),
                    new DataColumn("ApprovalGroups", typeof(string)),
                    new DataColumn("StatusType", typeof(string))
                });

            foreach (var emp in ListEmployeeDto)
            {
                dt.Rows.Add(
                    emp.Id,
                    emp.FullName,
                    emp.EmpCode,
                    emp.BankAccount,
                    emp.BankCardNo,
                    emp.PassportNo,
                    emp.TaxNumber,
                    emp.Nationality,
                    emp.DOB,
                    emp.DOJ,
                    emp.Gender,
                    emp.Email,
                    emp.MobileNumber,
                    emp.EmploymentType,
                    emp.BusinessUnits,
                    emp.JobRoles,
                    emp.ApprovalGroups,
                    emp.StatusType
                    );
            }

            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook

            List<string> docUrls = new();
            var docUrl = GetExcel("GetAllEmployees", dt);

            docUrls.Add(docUrl);


            return Ok(docUrls);
        }

        [HttpPost]
        [ActionName("GetEmployeesData")]
        public async Task<IActionResult> GetEmployeesData(EmployeeSearchModel searchModel)
        {

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<Employee>();


            if (searchModel.EmployeeId != 0 && searchModel.EmployeeId != null)
                predicate = predicate.And(x => x.Id == searchModel.EmployeeId);
            if (!string.IsNullOrEmpty(searchModel.EmployeeName))
                predicate = predicate.And(x => x.GetFullName().Contains(searchModel.EmployeeName));
            if (!string.IsNullOrEmpty(searchModel.EmpCode))
                predicate = predicate.And(x => x.EmpCode.Contains(searchModel.EmpCode));
            if (!string.IsNullOrEmpty(searchModel.Nationality))
                predicate = predicate.And(x => x.Nationality.Contains(searchModel.Nationality));
            if (!string.IsNullOrEmpty(searchModel.Gender))
                predicate = predicate.And(x => x.Gender.Contains(searchModel.Gender));
            if (searchModel.EmploymentTypeId != 0 && searchModel.EmploymentTypeId != null)
                predicate = predicate.And(x => x.EmploymentTypeId == searchModel.EmploymentTypeId);
            //if (searchModel.DepartmentId != 0 && searchModel.DepartmentId != null)
            //    predicate = predicate.And(x => x.DepartmentId == searchModel.DepartmentId);
            //if (searchModel.JobRoleId != 0 && searchModel.JobRoleId != null)
            //    predicate = predicate.And(x => x.RoleId == searchModel.JobRoleId);
            //if (searchModel.ApprovalGroupId != 0 && searchModel.ApprovalGroupId != null)
            //    predicate = predicate.And(x => x.ApprovalGroupId == searchModel.ApprovalGroupId);
            if (searchModel.StatusTypeId != 0 && searchModel.StatusTypeId != null)
                predicate = predicate.And(x => x.StatusTypeId == searchModel.StatusTypeId);


            List<Employee> result = new();


            if (predicate.IsStarted)
            {
                result = _context.Employees.Where(predicate).ToList();
            }
            else
            {
                result = _context.Employees.ToList();
            }


            List<EmployeeDTO> ListEmployeeDto = new();
            await Task.Run(() =>
            {

                foreach (Employee employee in result)
                {
                    EmployeeDTO employeeDTO = new();

                    employeeDTO.Id = employee.Id;
                    employeeDTO.FullName = employee.GetFullName();
                    employeeDTO.EmpCode = employee.EmpCode;
                    employeeDTO.BankAccount = employee.BankAccount;
                    employeeDTO.BankCardNo = employee.BankCardNo;
                    employeeDTO.PassportNo = employee.PassportNo;
                    employeeDTO.TaxNumber = employee.TaxNumber;
                    employeeDTO.Nationality = employee.Nationality;
                    employeeDTO.DOB = employee.DOB;
                    employeeDTO.DOJ = employee.DOJ;
                    employeeDTO.Gender = employee.Gender;
                    employeeDTO.Email = employee.Email;
                    employeeDTO.MobileNumber = employee.MobileNumber;
                    //employeeDTO.EmploymentType = employee.EmploymentTypeId != 0 ? _context.EmploymentTypes.Find(employee.EmploymentTypeId).EmpJobTypeCode + ":" + _context.EmploymentTypes.Find(employee.EmploymentTypeId).EmpJobTypeDesc : string.Empty;
                    //employeeDTO.Business Unit = employee.DepartmentId != 0 ? _context.Departments.Find(employee.DepartmentId).DeptCode + ":" + _context.Departments.Find(employee.DepartmentId).DeptName : string.Empty;
                    //employeeDTO.JobRole = employee.RoleId != 0 ? _context.JobRoles.Find(employee.RoleId).RoleCode + ":" + _context.JobRoles.Find(employee.RoleId).RoleName : string.Empty;
                    //employeeDTO.ApprovalGroup = employee.ApprovalGroupId != 0 ? _context.ApprovalGroups.Find(employee.ApprovalGroupId).ApprovalGroupCode : string.Empty;
                    employeeDTO.StatusType = employee.StatusTypeId != 0 ? _context.StatusTypes.Find(employee.StatusTypeId).Status : string.Empty;

                    ListEmployeeDto.Add(employeeDTO);
                }
            });


            return Ok(ListEmployeeDto);
        }


        [HttpPost]
        [ActionName("GetAdvanceAndReimburseReportsEmployeeJson")]

        public async Task<IActionResult> GetAdvanceAndReimburseReportsEmployeeJson(CashAndClaimRequestSearchModel searchModel)
        {
            int? empid = searchModel.LoggedEmpId;
            int? reporteeEmpId = searchModel.ReporteeEmpId;


            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<DisbursementsAndClaimsMaster>();

            if (empid == null || empid == 0)
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
            }

            // all employees who report under the manager
            //if Admin then show all the employee reports irrespective of the Business Unit.
            string empEmailId = _context.Employees.Find(empid).Email;
            var user = await userManager.FindByEmailAsync(empEmailId);
            bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");

            List<DisbursementsAndClaimsMaster> result = new();


            if (searchModel.RequestTypeId != 0 && searchModel.RequestTypeId != null)
                predicate = predicate.And(x => x.RequestTypeId == searchModel.RequestTypeId);
            if (searchModel.BusinessTypeId != 0 && searchModel.BusinessTypeId != null)
                predicate = predicate.And(x => x.BusinessTypeId == searchModel.BusinessTypeId);
            if (searchModel.BusinessUnitId != 0 && searchModel.BusinessUnitId != null)
                predicate = predicate.And(x => x.BusinessUnitId == searchModel.BusinessUnitId);
            if (searchModel.ProjectId != 0 && searchModel.ProjectId != null)
                predicate = predicate.And(x => x.ProjectId == searchModel.ProjectId);
            if (searchModel.SubProjectId != 0 && searchModel.SubProjectId != null)
                predicate = predicate.And(x => x.SubProjectId == searchModel.SubProjectId);
            if (searchModel.RecordDateFrom.HasValue)
                predicate = predicate.And(x => x.RecordDate >= searchModel.RecordDateFrom);
            if (searchModel.RecordDateTo.HasValue)
                predicate = predicate.And(x => x.RecordDate <= searchModel.RecordDateTo);
            if (searchModel.AmountFrom > 0)
                predicate = predicate.And(x => x.ClaimAmount >= searchModel.AmountFrom);
            if (searchModel.AmountTo > 0)
                predicate = predicate.And(x => x.ClaimAmount <= searchModel.AmountTo);
            if (searchModel.IsAccountSettled != null)
                predicate = predicate.And(x => x.IsSettledAmountCredited == searchModel.IsAccountSettled);
            if (searchModel.CostCenterId != 0 && searchModel.CostCenterId != null)
                predicate = predicate.And(x => x.CostCenterId == searchModel.CostCenterId);
            if (searchModel.ApprovalStatusId != 0 && searchModel.ApprovalStatusId != null)
                predicate = predicate.And(x => x.ApprovalStatusId == searchModel.ApprovalStatusId);

            if (reporteeEmpId != null && searchModel.IsManager == true)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }
            else if (reporteeEmpId == null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == empid);
            }
            else if (reporteeEmpId != null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }


            if (isAdmin)
            {
                result = _context.DisbursementsAndClaimsMasters.Where(predicate).OrderBy(e => e.RecordDate).ToList();

            }
            else if (searchModel.IsManager)
            {
                result = _context.DisbursementsAndClaimsMasters.Where(predicate).OrderBy(e => e.RecordDate).ToList();

                //var mgrDeptId = _context.Employees.Find(empid).DepartmentId;
                List<int> mgrProjects = _context.ProjectManagements.Where(x => x.EmployeeId == empid).Select(p => p.ProjectId).ToList(); //if projManager get projects
                //using parameter empId => get List of business units (where employee is a manager)
                var mgrLinkedBusinessUnits = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == empid).Select(s => s.BusinessUnitId).ToList();
                // Find all employee Ids from the business units to apply filter (to find reportees to the manager)
                List<int> mgrReportees = _context.EmployeeExtendedInfos.Where(r => mgrLinkedBusinessUnits.Contains(r.BusinessUnitId)).Select(s => s.EmployeeId).Distinct().ToList();


                result = result.Where(r => mgrReportees.Contains(r.EmployeeId) || mgrProjects.Contains(r.ProjectId ?? 0)).OrderBy(e => e.RecordDate).ToList();


            }
            else if (!searchModel.IsManager)
            {
               
                result = _context.DisbursementsAndClaimsMasters.Where(predicate).OrderBy(e => e.RecordDate).ToList();
            }



            List<DisbursementsAndClaimsMasterDTO> ListDisbItemsDTO = new();

            //await Task.Run(() =>
            //{
            foreach (DisbursementsAndClaimsMaster disb in result)
            {
                DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO = new();
                disbursementsAndClaimsMasterDTO.Id = disb.Id;
                disbursementsAndClaimsMasterDTO.EmployeeId = disb.EmployeeId;
                disbursementsAndClaimsMasterDTO.EmployeeName = _context.Employees.Find(disb.EmployeeId).GetFullName();
                disbursementsAndClaimsMasterDTO.BlendedRequestId = disb.BlendedRequestId;
                disbursementsAndClaimsMasterDTO.RequestTypeId = disb.RequestTypeId;
                disbursementsAndClaimsMasterDTO.RequestType = _context.RequestTypes.Find(disb.RequestTypeId).RequestName;



                disbursementsAndClaimsMasterDTO.BusinessTypeId = disb.BusinessTypeId;
                disbursementsAndClaimsMasterDTO.BusinessType = disbursementsAndClaimsMasterDTO.BusinessTypeId != null ? _context.BusinessTypes.Find(disb.BusinessTypeId).BusinessTypeName : null;
                disbursementsAndClaimsMasterDTO.BusinessUnitId = disb.BusinessUnitId;
                disbursementsAndClaimsMasterDTO.BusinessUnit = disb.BusinessUnitId != null ? _context.BusinessUnits.Find(disb.BusinessUnitId).GetBusinessUnitName() : null;



                disbursementsAndClaimsMasterDTO.ProjectId = disb.ProjectId;
                disbursementsAndClaimsMasterDTO.ProjectName = disb.ProjectId != null ? _context.Projects.Find(disb.ProjectId).ProjectName : null;
                disbursementsAndClaimsMasterDTO.SubProjectId = disb.SubProjectId;
                disbursementsAndClaimsMasterDTO.SubProjectName = disb.SubProjectId != null ? _context.SubProjects.Find(disb.SubProjectId).SubProjectName : null;
                disbursementsAndClaimsMasterDTO.WorkTaskId = disb.WorkTaskId;
                disbursementsAndClaimsMasterDTO.WorkTaskName = disb.WorkTaskId != null ? _context.WorkTasks.Find(disb.WorkTaskId).TaskName : null;
                disbursementsAndClaimsMasterDTO.CurrencyTypeId = disb.CurrencyTypeId;
                disbursementsAndClaimsMasterDTO.CurrencyType = disb.CurrencyTypeId != 0 ? _context.CurrencyTypes.Find(disb.CurrencyTypeId).CurrencyCode : null;
                disbursementsAndClaimsMasterDTO.ClaimAmount = disb.ClaimAmount;
                disbursementsAndClaimsMasterDTO.AmountToWallet = disb.AmountToWallet ?? 0;
                disbursementsAndClaimsMasterDTO.AmountToCredit = disb.AmountToCredit ?? 0;
                disbursementsAndClaimsMasterDTO.CostCenterId = disb.CostCenterId;
                disbursementsAndClaimsMasterDTO.CostCenter = disb.CostCenterId != 0 ? _context.CostCenters.Find(disb.CostCenterId).GetCostCentre() : null;
                disbursementsAndClaimsMasterDTO.ApprovalStatusId = disb.ApprovalStatusId;
                disbursementsAndClaimsMasterDTO.ApprovalStatusType = disb.ApprovalStatusId != 0 ? _context.ApprovalStatusTypes.Find(disb.ApprovalStatusId).Status : null;
                disbursementsAndClaimsMasterDTO.RequestDate = disb.RecordDate;
                disbursementsAndClaimsMasterDTO.IsSettledAmountCredited = disb.IsSettledAmountCredited ?? false;
                disbursementsAndClaimsMasterDTO.SettledDate = disb.SettledDate != null ? disb.SettledDate.Value.ToShortDateString() : string.Empty;
                disbursementsAndClaimsMasterDTO.SettlementComment = disb.SettlementComment;
                disbursementsAndClaimsMasterDTO.SettlementAccount = disb.SettlementAccount;
                disbursementsAndClaimsMasterDTO.SettlementBankCard = disb.SettlementBankCard;
                disbursementsAndClaimsMasterDTO.AdditionalData = disb.AdditionalData;

                if (searchModel.RequestTypeId == 1)
                {
                    disbursementsAndClaimsMasterDTO.RequestTitleDescription = _context.CashAdvanceRequests.Find(disb.BlendedRequestId).Comments;
                }
                else
                {
                    disbursementsAndClaimsMasterDTO.RequestTitleDescription = _context.ExpenseReimburseRequests.Find(disb.BlendedRequestId).ExpenseReportTitle;
                }

                ListDisbItemsDTO.Add(disbursementsAndClaimsMasterDTO);
            }

            //});
            return Ok(ListDisbItemsDTO);

        }


        [HttpPost]
        [ActionName("GetAdvanceAndReimburseReportsEmployeeExcel")]

        public async Task<IActionResult> GetAdvanceAndReimburseReportsEmployeeExcel(CashAndClaimRequestSearchModel searchModel)
        {
            int? empid = searchModel.LoggedEmpId;
            int? reporteeEmpId = searchModel.ReporteeEmpId;


            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<DisbursementsAndClaimsMaster>();

            if (empid == null || empid == 0)
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
            }

            // all employees who report under the manager
            //if Admin then show all the employee reports irrespective of the Business Unit.
            string empEmailId = _context.Employees.Find(empid).Email;
            var user = await userManager.FindByEmailAsync(empEmailId);
            bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");

            List<DisbursementsAndClaimsMaster> result = new();


            if (searchModel.RequestTypeId != 0 && searchModel.RequestTypeId != null)
                predicate = predicate.And(x => x.RequestTypeId == searchModel.RequestTypeId);
            if (searchModel.BusinessTypeId != 0 && searchModel.BusinessTypeId != null)
                predicate = predicate.And(x => x.BusinessTypeId == searchModel.BusinessTypeId);
            if (searchModel.BusinessUnitId != 0 && searchModel.BusinessUnitId != null)
                predicate = predicate.And(x => x.BusinessUnitId == searchModel.BusinessUnitId);
            if (searchModel.ProjectId != 0 && searchModel.ProjectId != null)
                predicate = predicate.And(x => x.ProjectId == searchModel.ProjectId);
            if (searchModel.SubProjectId != 0 && searchModel.SubProjectId != null)
                predicate = predicate.And(x => x.SubProjectId == searchModel.SubProjectId);
            if (searchModel.RecordDateFrom.HasValue)
                predicate = predicate.And(x => x.RecordDate >= searchModel.RecordDateFrom);
            if (searchModel.RecordDateTo.HasValue)
                predicate = predicate.And(x => x.RecordDate <= searchModel.RecordDateTo);
            if (searchModel.AmountFrom > 0)
                predicate = predicate.And(x => x.ClaimAmount >= searchModel.AmountFrom);
            if (searchModel.AmountTo > 0)
                predicate = predicate.And(x => x.ClaimAmount <= searchModel.AmountTo);
            if (searchModel.IsAccountSettled != null)
                predicate = predicate.And(x => x.IsSettledAmountCredited == searchModel.IsAccountSettled);
            if (searchModel.CostCenterId != 0 && searchModel.CostCenterId != null)
                predicate = predicate.And(x => x.CostCenterId == searchModel.CostCenterId);
            if (searchModel.ApprovalStatusId != 0 && searchModel.ApprovalStatusId != null)
                predicate = predicate.And(x => x.ApprovalStatusId == searchModel.ApprovalStatusId);

            if (reporteeEmpId != null && searchModel.IsManager == true)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }
            else if (reporteeEmpId == null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == empid);
            }
            else if (reporteeEmpId != null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }


            if (isAdmin)
            {
                result = _context.DisbursementsAndClaimsMasters.Where(predicate).OrderBy(e => e.RecordDate).ToList();

            }
            else if (searchModel.IsManager)
            {
                result = _context.DisbursementsAndClaimsMasters.Where(predicate).OrderBy(e => e.RecordDate).ToList();

                //var mgrDeptId = _context.Employees.Find(empid).DepartmentId;
                List<int> mgrProjects = _context.ProjectManagements.Where(x => x.EmployeeId == empid).Select(p => p.ProjectId).ToList(); //if projManager get projects
                //using parameter empId => get List of business units (where employee is a manager)
                var mgrLinkedBusinessUnits = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == empid).Select(s => s.BusinessUnitId).ToList();
                // Find all employee Ids from the business units to apply filter (to find reportees to the manager)
                List<int> mgrReportees = _context.EmployeeExtendedInfos.Where(r => mgrLinkedBusinessUnits.Contains(r.BusinessUnitId)).Select(s => s.EmployeeId).Distinct().ToList();


                result = result.Where(r => mgrReportees.Contains(r.EmployeeId) || mgrProjects.Contains(r.ProjectId ?? 0)).OrderBy(e => e.RecordDate).ToList();


            }
            else if (!searchModel.IsManager)
            {
                

                result = _context.DisbursementsAndClaimsMasters.Where(predicate).OrderBy(e => e.RecordDate).ToList();
            }


            List<DisbursementsAndClaimsMasterDTO> ListDisbItemsDTO = new();

            await Task.Run(() =>
            {
                foreach (DisbursementsAndClaimsMaster disb in result.OrderByDescending(x => x.RecordDate).ToList())
                {
                    DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO = new();
                    disbursementsAndClaimsMasterDTO.Id = disb.Id;
                    disbursementsAndClaimsMasterDTO.EmployeeId = disb.EmployeeId;
                    disbursementsAndClaimsMasterDTO.EmployeeName = _context.Employees.Find(disb.EmployeeId).GetFullName();
                    disbursementsAndClaimsMasterDTO.BlendedRequestId = disb.BlendedRequestId;
                    disbursementsAndClaimsMasterDTO.RequestTypeId = disb.RequestTypeId;
                    disbursementsAndClaimsMasterDTO.RequestType = _context.RequestTypes.Find(disb.RequestTypeId).RequestName;


                    disbursementsAndClaimsMasterDTO.BusinessTypeId = disb.BusinessTypeId;
                    disbursementsAndClaimsMasterDTO.BusinessType = disbursementsAndClaimsMasterDTO.BusinessTypeId != null ? _context.BusinessTypes.Find(disb.BusinessTypeId).BusinessTypeName : null;
                    disbursementsAndClaimsMasterDTO.BusinessUnitId = disb.BusinessUnitId;
                    disbursementsAndClaimsMasterDTO.BusinessUnit = disb.BusinessUnitId != null ? _context.BusinessUnits.Find(disb.BusinessUnitId).GetBusinessUnitName() : null;


                    disbursementsAndClaimsMasterDTO.ProjectId = disb.ProjectId;
                    disbursementsAndClaimsMasterDTO.ProjectName = disb.ProjectId != null ? _context.Projects.Find(disb.ProjectId).ProjectName : null;
                    disbursementsAndClaimsMasterDTO.SubProjectId = disb.SubProjectId;
                    disbursementsAndClaimsMasterDTO.SubProjectName = disb.SubProjectId != null ? _context.SubProjects.Find(disb.SubProjectId).SubProjectName : null;
                    disbursementsAndClaimsMasterDTO.WorkTaskId = disb.WorkTaskId;
                    disbursementsAndClaimsMasterDTO.WorkTaskName = disb.WorkTaskId != null ? _context.WorkTasks.Find(disb.WorkTaskId).TaskName : null;
                    disbursementsAndClaimsMasterDTO.CurrencyTypeId = disb.CurrencyTypeId;
                    disbursementsAndClaimsMasterDTO.CurrencyType = disb.CurrencyTypeId != 0 ? _context.CurrencyTypes.Find(disb.CurrencyTypeId).CurrencyCode : null;
                    disbursementsAndClaimsMasterDTO.ClaimAmount = disb.ClaimAmount;
                    disbursementsAndClaimsMasterDTO.AmountToWallet = disb.AmountToWallet ?? 0;
                    disbursementsAndClaimsMasterDTO.AmountToCredit = disb.AmountToCredit ?? 0;
                    disbursementsAndClaimsMasterDTO.CostCenterId = disb.CostCenterId;
                    disbursementsAndClaimsMasterDTO.CostCenter = disb.CostCenterId != 0 ? _context.CostCenters.Find(disb.CostCenterId).GetCostCentre() : null;
                    disbursementsAndClaimsMasterDTO.ApprovalStatusId = disb.ApprovalStatusId;
                    disbursementsAndClaimsMasterDTO.ApprovalStatusType = disb.ApprovalStatusId != 0 ? _context.ApprovalStatusTypes.Find(disb.ApprovalStatusId).Status : null;
                    disbursementsAndClaimsMasterDTO.RequestDate = disb.RecordDate;
                    disbursementsAndClaimsMasterDTO.IsSettledAmountCredited = disb.IsSettledAmountCredited ?? false;
                    disbursementsAndClaimsMasterDTO.SettledDate = disb.SettledDate != null ? disb.SettledDate.Value.ToShortDateString() : string.Empty;
                    disbursementsAndClaimsMasterDTO.SettlementComment = disb.SettlementComment;
                    disbursementsAndClaimsMasterDTO.SettlementAccount = disb.SettlementAccount;
                    disbursementsAndClaimsMasterDTO.SettlementBankCard = disb.SettlementBankCard;
                    disbursementsAndClaimsMasterDTO.AdditionalData = disb.AdditionalData;


                    if (searchModel.RequestTypeId == 1)
                    {
                        disbursementsAndClaimsMasterDTO.RequestTitleDescription = _context.CashAdvanceRequests.Find(disb.BlendedRequestId).Comments;
                    }
                    else
                    {
                        disbursementsAndClaimsMasterDTO.RequestTitleDescription = _context.ExpenseReimburseRequests.Find(disb.BlendedRequestId).ExpenseReportTitle;
                    }

                    ListDisbItemsDTO.Add(disbursementsAndClaimsMasterDTO);
                }

            });
            //return Ok(ListDisbItemsDTO);


            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[22]
                {
                    //new DataColumn("Id", typeof(int)),
                    new DataColumn("EmployeeName", typeof(string)),
                     new DataColumn("BlendedRequestId", typeof(int)),
                    new DataColumn("RequestType",typeof(string)),
                    new DataColumn("BusinessType",typeof(string)),
                    new DataColumn("BusinessUnit(s)",typeof(string)),
                    new DataColumn("Project",typeof(string)),
                    new DataColumn("SubProject", typeof(string)),
                    new DataColumn("WorkTask",typeof(string)),
                    new DataColumn("RecordDate",typeof(string)),
                    new DataColumn("CurrencyType",typeof(string)),
                    new DataColumn("ClaimAmount", typeof(Double)),
                    new DataColumn("AmountToWallet", typeof(Double)),
                    new DataColumn("AmountToCredit", typeof(Double)),
                    new DataColumn("CostCenter", typeof(string)),
                    new DataColumn("ApprovalStatus", typeof(string)),
                    new DataColumn("IsSettledAmountCredited", typeof(bool)),
                    new DataColumn("SettledDate", typeof(string)),
                    new DataColumn("SettlementComment", typeof(string)),
                    new DataColumn("SettlementAccount", typeof(string)),
                    new DataColumn("SettlementBankCard", typeof(string)),
                    new DataColumn("AdditionalData", typeof(string)),
                    new DataColumn("TitleDescription", typeof(string))
                });


            foreach (var disbItem in ListDisbItemsDTO)
            {
                dt.Rows.Add(
                    disbItem.EmployeeName,
                    disbItem.BlendedRequestId,
                    disbItem.RequestType,
                    disbItem.BusinessType,
                    disbItem.BusinessUnit,
                    disbItem.ProjectName,
                    disbItem.SubProjectName,
                    disbItem.WorkTaskName,
                    disbItem.RequestDate,
                    disbItem.CurrencyType,
                    disbItem.ClaimAmount,
                    disbItem.AmountToWallet,
                    disbItem.AmountToCredit,
                    disbItem.CostCenter,
                    disbItem.ApprovalStatusType,
                    disbItem.IsSettledAmountCredited,
                    disbItem.SettledDate,
                    disbItem.SettlementComment,
                    disbItem.SettlementAccount,
                    disbItem.SettlementBankCard,
                    disbItem.AdditionalData,
                    disbItem.RequestTitleDescription
                    );
            }
            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook

            List<string> docUrls = new();
            var docUrl = GetExcel("CashRequestAndClaimsReport", dt);

            docUrls.Add(docUrl);

            return Ok(docUrls);
        }




        [HttpPost]
        [ActionName("GetTravelRequestReportForEmployeeJson")]

        public async Task<IActionResult> GetTravelRequestReportForEmployeeJson(TravelRequestSearchModel searchModel)
        {
            int? empid = searchModel.LoggedEmpId;
            int? reporteeEmpId = searchModel.ReporteeEmpId;


            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<TravelApprovalRequest>();

            if (empid == null || empid == 0)
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
            }

            // all employees who report under the manager
            //if Admin then show all the employee reports irrespective of the Business Unit.
            string empEmailId = _context.Employees.Find(empid).Email;
            var user = await userManager.FindByEmailAsync(empEmailId);
            bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");

            List<TravelApprovalRequest> result = new();


            if (searchModel.TravelApprovalRequestId != 0 && searchModel.TravelApprovalRequestId != null)
                predicate = predicate.And(x => x.Id == searchModel.TravelApprovalRequestId);
            if (searchModel.TravelStartDate.HasValue)
                predicate = predicate.And(x => x.TravelStartDate >= searchModel.TravelStartDate);
            if (searchModel.TravelEndDate.HasValue)
                predicate = predicate.And(x => x.TravelEndDate <= searchModel.TravelEndDate);
            if (!string.IsNullOrEmpty(searchModel.TravelPurpose))
                predicate = predicate.And(x => x.TravelPurpose.Contains(searchModel.TravelPurpose));
            if (searchModel.BusinessUnitId != 0 && searchModel.BusinessUnitId != null)
                predicate = predicate.And(x => x.BusinessUnitId == searchModel.BusinessUnitId);
            if (searchModel.ProjectId != 0 && searchModel.ProjectId != null)
                predicate = predicate.And(x => x.ProjectId == searchModel.ProjectId);
            if (searchModel.ReqRaisedDate.HasValue)
                predicate = predicate.And(x => x.RequestDate >= searchModel.ReqRaisedDate);
            if (searchModel.ReqRaisedDate.HasValue)
                predicate = predicate.And(x => x.RequestDate <= searchModel.ReqRaisedDate);
            if (searchModel.ApprovalStatusTypeId != 0 && searchModel.ApprovalStatusTypeId != null)
                predicate = predicate.And(x => x.ApprovalStatusTypeId == searchModel.ApprovalStatusTypeId);


            if (reporteeEmpId != null && searchModel.IsManager == true)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }
            else if (reporteeEmpId == null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == empid);
            }
            else if (reporteeEmpId != null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }

            if (isAdmin)
            {
                result = predicate.IsStarted ? _context.TravelApprovalRequests.Where(predicate).OrderBy(e => e.RequestDate).ToList() : _context.TravelApprovalRequests.OrderBy(e => e.RequestDate).ToList();

            }
            else if (searchModel.IsManager)
            {
                result = predicate.IsStarted ? _context.TravelApprovalRequests.Where(predicate).OrderBy(e => e.RequestDate).ToList() : _context.TravelApprovalRequests.OrderBy(e => e.RequestDate).ToList();


                //var mgrDeptId = _context.Employees.Find(empid).DepartmentId;
                List<int> mgrProjects = _context.ProjectManagements.Where(x => x.EmployeeId == empid).Select(p => p.ProjectId).ToList(); //if projManager get projects
                //using parameter empId => get List of business units (where employee is a manager)
                var mgrLinkedBusinessUnits = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == empid).Select(s => s.BusinessUnitId).ToList();
                // Find all employee Ids from the business units to apply filter (to find reportees to the manager)
                List<int> mgrReportees = _context.EmployeeExtendedInfos.Where(r => mgrLinkedBusinessUnits.Contains(r.BusinessUnitId)).Select(s => s.EmployeeId).Distinct().ToList();


                result = result.Where(r => mgrReportees.Contains(r.EmployeeId ?? 0) || mgrProjects.Contains(r.ProjectId ?? 0)).OrderBy(e => e.RequestDate).ToList();

            }
            else if (!searchModel.IsManager)
            {
                

                result = _context.TravelApprovalRequests.Where(predicate).OrderBy(e => e.RequestDate).ToList();
            }


            List<TravelApprovalRequestDTO> ListTravelItemsDTO = new();

            await Task.Run(() =>
            {
                foreach (TravelApprovalRequest travel in result)
                {
                    TravelApprovalRequestDTO travelItemDTO = new();
                    travelItemDTO.Id = travel.Id;
                    travelItemDTO.EmployeeId = travel.EmployeeId;
                    travelItemDTO.EmployeeName = _context.Employees.Find(travel.EmployeeId).GetFullName();



                    travelItemDTO.BusinessTypeId = travel.BusinessTypeId;
                    travelItemDTO.BusinessType = travelItemDTO.BusinessTypeId != null ? _context.BusinessTypes.Find(travel.BusinessTypeId).BusinessTypeName : null;
                    travelItemDTO.BusinessUnitId = travel.BusinessUnitId;
                    travelItemDTO.BusinessUnit = travel.BusinessUnitId != null ? _context.BusinessUnits.Find(travel.BusinessUnitId).GetBusinessUnitName() : null;


                    travelItemDTO.ProjectId = travel.ProjectId;
                    travelItemDTO.ProjectName = travel.ProjectId != null ? _context.Projects.Find(travel.ProjectId).ProjectName : null;
                    travelItemDTO.SubProjectId = travel.SubProjectId;
                    travelItemDTO.SubProjectName = travel.SubProjectId != null ? _context.SubProjects.Find(travel.SubProjectId).SubProjectName : null;
                    travelItemDTO.WorkTaskId = travel.WorkTaskId;
                    travelItemDTO.WorkTaskName = travel.WorkTaskId != null ? _context.WorkTasks.Find(travel.WorkTaskId).TaskName : null;
                    travelItemDTO.CostCenterId = travel.CostCenterId;
                    travelItemDTO.CostCenter = travel.CostCenterId != 0 ? _context.CostCenters.Find(travel.CostCenterId).GetCostCentre() : null;
                    travelItemDTO.ApprovalStatusTypeId = travel.ApprovalStatusTypeId;
                    travelItemDTO.ApprovalStatusType = travel.ApprovalStatusTypeId != 0 ? _context.ApprovalStatusTypes.Find(travel.ApprovalStatusTypeId).Status : null;
                    travelItemDTO.RequestDate = travel.RequestDate;

                    ListTravelItemsDTO.Add(travelItemDTO);
                }
            });

            return Ok(ListTravelItemsDTO);

        }



        [HttpPost]
        [ActionName("GetTravelRequestReportForEmployeeExcel")]
        public async Task<IActionResult> GetTravelRequestReportForEmployeeExcel(TravelRequestSearchModel searchModel)
        {

            int? empid = searchModel.LoggedEmpId;
            int? reporteeEmpId = searchModel.ReporteeEmpId;


            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<TravelApprovalRequest>();

            if (empid == null || empid == 0)
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
            }

            // all employees who report under the manager
            //if Admin then show all the employee reports irrespective of the Business Unit.
            string empEmailId = _context.Employees.Find(empid).Email;
            var user = await userManager.FindByEmailAsync(empEmailId);
            bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");

            List<TravelApprovalRequest> result = new();


            if (searchModel.TravelApprovalRequestId != 0 && searchModel.TravelApprovalRequestId != null)
                predicate = predicate.And(x => x.Id == searchModel.TravelApprovalRequestId);
            if (searchModel.TravelStartDate.HasValue)
                predicate = predicate.And(x => x.TravelStartDate >= searchModel.TravelStartDate);
            if (searchModel.TravelEndDate.HasValue)
                predicate = predicate.And(x => x.TravelEndDate <= searchModel.TravelEndDate);
            if (!string.IsNullOrEmpty(searchModel.TravelPurpose))
                predicate = predicate.And(x => x.TravelPurpose.Contains(searchModel.TravelPurpose));
            if (searchModel.BusinessUnitId != 0 && searchModel.BusinessUnitId != null)
                predicate = predicate.And(x => x.BusinessUnitId == searchModel.BusinessUnitId);
            if (searchModel.ProjectId != 0 && searchModel.ProjectId != null)
                predicate = predicate.And(x => x.ProjectId == searchModel.ProjectId);
            if (searchModel.ReqRaisedDate.HasValue)
                predicate = predicate.And(x => x.RequestDate >= searchModel.ReqRaisedDate);
            if (searchModel.ReqRaisedDate.HasValue)
                predicate = predicate.And(x => x.RequestDate <= searchModel.ReqRaisedDate);
            if (searchModel.ApprovalStatusTypeId != 0 && searchModel.ApprovalStatusTypeId != null)
                predicate = predicate.And(x => x.ApprovalStatusTypeId == searchModel.ApprovalStatusTypeId);

            if (reporteeEmpId != null && searchModel.IsManager == true)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }
            else if (reporteeEmpId == null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == empid);
            }
            else if (reporteeEmpId != null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }

            if (isAdmin)
            {
                result = predicate.IsStarted ? _context.TravelApprovalRequests.Where(predicate).OrderBy(e => e.RequestDate).ToList() : _context.TravelApprovalRequests.OrderBy(e => e.RequestDate).ToList();

            }
            else if (searchModel.IsManager)
            {
                result = predicate.IsStarted ? _context.TravelApprovalRequests.Where(predicate).OrderBy(e => e.RequestDate).ToList() : _context.TravelApprovalRequests.OrderBy(e => e.RequestDate).ToList();

                List<int> mgrProjects = _context.ProjectManagements.Where(x => x.EmployeeId == empid).Select(p => p.ProjectId).ToList(); //if projManager get projects
                //using parameter empId => get List of business units (where employee is a manager)
                var mgrLinkedBusinessUnits = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == empid).Select(s => s.BusinessUnitId).ToList();
                // Find all employee Ids from the business units to apply filter (to find reportees to the manager)
                List<int> mgrReportees = _context.EmployeeExtendedInfos.Where(r => mgrLinkedBusinessUnits.Contains(r.BusinessUnitId)).Select(s => s.EmployeeId).Distinct().ToList();


                result = result.Where(r => mgrReportees.Contains(r.EmployeeId ?? 0) || mgrProjects.Contains(r.ProjectId ?? 0)).OrderBy(e => e.RequestDate).ToList();

            }
            else if (!searchModel.IsManager)
            {
                

                result = _context.TravelApprovalRequests.Where(predicate).OrderBy(e => e.RequestDate).ToList();
            }



            List<TravelApprovalRequestDTO> ListTravelItemsDTO = new();
            await Task.Run(() =>
            {

                foreach (TravelApprovalRequest travel in result)
                {
                    TravelApprovalRequestDTO travelItemDTO = new();
                    travelItemDTO.Id = travel.Id;
                    travelItemDTO.EmployeeId = travel.EmployeeId;
                    travelItemDTO.EmployeeName = _context.Employees.Find(travel.EmployeeId).GetFullName();

                    travelItemDTO.TravelStartDate = travel.TravelStartDate;
                    travelItemDTO.TravelEndDate = travel.TravelEndDate;
                    travelItemDTO.TravelPurpose = travel.TravelPurpose;
                    travelItemDTO.BusinessUnitId = travel.BusinessUnitId;
                    travelItemDTO.BusinessUnit = _context.BusinessUnits.Find(travel.BusinessUnit).GetBusinessUnitName();

                    travelItemDTO.ProjectId = travel.ProjectId;
                    travelItemDTO.ProjectName = travel.ProjectId != null ? _context.Projects.Find(travel.ProjectId).ProjectName : null;
                    travelItemDTO.SubProjectId = travel.SubProjectId;
                    travelItemDTO.SubProjectName = travel.SubProjectId != null ? _context.SubProjects.Find(travel.SubProjectId).SubProjectName : null;
                    travelItemDTO.WorkTaskId = travel.WorkTaskId;
                    travelItemDTO.WorkTaskName = travel.WorkTaskId != null ? _context.WorkTasks.Find(travel.WorkTaskId).TaskName : null;
                    travelItemDTO.CostCenterId = travel.CostCenterId;
                    travelItemDTO.CostCenter = travel.CostCenterId != 0 ? _context.CostCenters.Find(travel.CostCenterId).CostCenterCode : null;
                    travelItemDTO.ApprovalStatusTypeId = travel.ApprovalStatusTypeId;
                    travelItemDTO.ApprovalStatusType = travel.ApprovalStatusTypeId != 0 ? _context.ApprovalStatusTypes.Find(travel.ApprovalStatusTypeId).Status : null;
                    travelItemDTO.RequestDate = travel.RequestDate;

                    ListTravelItemsDTO.Add(travelItemDTO);
                }
            });


            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[12]
                {
                    new DataColumn("TravelRequestId", typeof(int)),
                    new DataColumn("EmployeeName", typeof(string)),
                    new DataColumn("TravelStartDate",typeof(string)),
                    new DataColumn("TravelEndDate",typeof(string)),
                    new DataColumn("TravelPurpose",typeof(string)),
                    new DataColumn("BusinessUnit(s)",typeof(string)),
                    new DataColumn("Project",typeof(string)),
                    new DataColumn("SubProject", typeof(string)),
                    new DataColumn("WorkTask",typeof(string)),
                    new DataColumn("RequestDate",typeof(string)),
                    new DataColumn("CostCenter", typeof(string)),
                    new DataColumn("ApprovalStatus", typeof(string))
                });

            foreach (var travelItem in ListTravelItemsDTO)
            {
                dt.Rows.Add
                    (
                    travelItem.Id,
                    travelItem.EmployeeName,
                    travelItem.TravelStartDate.Value.ToShortDateString(),
                    travelItem.TravelEndDate.Value.ToShortDateString(),
                    travelItem.TravelPurpose,
                    travelItem.BusinessUnit,
                    travelItem.ProjectName,
                    travelItem.SubProjectName,
                    travelItem.WorkTaskName,
                    travelItem.RequestDate.Value.ToShortDateString(),
                    travelItem.CostCenter,
                    travelItem.ApprovalStatusType
                    );
            }


            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook

            List<string> docUrls = new();
            var docUrl = GetExcel("TravelRequestReportForEmployee", dt);

            docUrls.Add(docUrl);

            return Ok(docUrls);
        }


        [HttpPost]
        [ActionName("AccountsPayableData")]
        public async Task<ActionResult<IEnumerable<DisbursementsAndClaimsMasterDTO>>> AccountsPayableData(AccountsPayableSearchModel searchModel)
        {


            List<DisbursementsAndClaimsMaster> result = new();
            List<DisbursementsAndClaimsMasterDTO> ListDisbursementsAndClaimsMasterDTO = new();

            // using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<DisbursementsAndClaimsMaster>();

            predicate = predicate.And(x => x.ApprovalStatusId == (int)EApprovalStatus.Approved);

            if (searchModel.IsAccountSettled == true || searchModel.IsAccountSettled == false)
            {
                predicate = predicate.And(x => x.IsSettledAmountCredited == searchModel.IsAccountSettled);
            }

            predicate = predicate.And(x => x.ApprovalStatusId == (int)EApprovalStatus.Approved);

            if (searchModel.SettledAccountsFrom.HasValue)
                predicate = predicate.And(x => x.RecordDate >= searchModel.SettledAccountsFrom);
            if (searchModel.SettledAccountsTo.HasValue)
                predicate = predicate.And(x => x.RecordDate <= searchModel.SettledAccountsTo);

            if (predicate.IsStarted)
            {
                result = _context.DisbursementsAndClaimsMasters.Where(predicate).OrderByDescending(x => x.RecordDate).ToList();
            }
            else
            {
                result = _context.DisbursementsAndClaimsMasters.OrderByDescending(x => x.RecordDate).ToList();
            }


            foreach (DisbursementsAndClaimsMaster disbursementsAndClaimsMaster in result)
            {
                DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO = new();

                disbursementsAndClaimsMasterDTO.Id = disbursementsAndClaimsMaster.Id;
                disbursementsAndClaimsMasterDTO.EmployeeId = disbursementsAndClaimsMaster.EmployeeId;
                disbursementsAndClaimsMasterDTO.EmployeeName = _context.Employees.Find(disbursementsAndClaimsMaster.EmployeeId).GetFullName();
                disbursementsAndClaimsMasterDTO.BlendedRequestId = disbursementsAndClaimsMaster.BlendedRequestId;


                disbursementsAndClaimsMasterDTO.BusinessTypeId = disbursementsAndClaimsMaster.BusinessTypeId;
                disbursementsAndClaimsMasterDTO.BusinessType = disbursementsAndClaimsMasterDTO.BusinessTypeId != null ? _context.BusinessTypes.Find(disbursementsAndClaimsMaster.BusinessTypeId).BusinessTypeName : null;
                disbursementsAndClaimsMasterDTO.BusinessUnitId = disbursementsAndClaimsMaster.BusinessUnitId;
                disbursementsAndClaimsMasterDTO.BusinessUnit = disbursementsAndClaimsMaster.BusinessUnitId != null ? _context.BusinessUnits.Find(disbursementsAndClaimsMaster.BusinessUnitId).GetBusinessUnitName() : null;

                disbursementsAndClaimsMasterDTO.ProjectId = disbursementsAndClaimsMaster.ProjectId;
                disbursementsAndClaimsMasterDTO.ProjectName = disbursementsAndClaimsMaster.ProjectId != null ? _context.Projects.Find(disbursementsAndClaimsMaster.ProjectId).ProjectName : string.Empty;
                disbursementsAndClaimsMasterDTO.SubProjectId = disbursementsAndClaimsMaster.SubProjectId;
                disbursementsAndClaimsMasterDTO.SubProjectName = disbursementsAndClaimsMaster.SubProjectId != null ? _context.SubProjects.Find(disbursementsAndClaimsMaster.SubProjectId).SubProjectName : string.Empty;
                disbursementsAndClaimsMasterDTO.WorkTaskId = disbursementsAndClaimsMaster.WorkTaskId;
                disbursementsAndClaimsMasterDTO.WorkTaskName = disbursementsAndClaimsMaster.WorkTaskId != null ? _context.WorkTasks.Find(disbursementsAndClaimsMaster.WorkTaskId).TaskName : string.Empty;
                disbursementsAndClaimsMasterDTO.CurrencyTypeId = disbursementsAndClaimsMaster.CurrencyTypeId;
                disbursementsAndClaimsMasterDTO.CurrencyType = _context.CurrencyTypes.Find(disbursementsAndClaimsMaster.CurrencyTypeId).CurrencyCode;
                disbursementsAndClaimsMasterDTO.RequestDate = disbursementsAndClaimsMaster.RecordDate == null ? null : disbursementsAndClaimsMaster.RecordDate; //ToShortDateString()
                disbursementsAndClaimsMasterDTO.ClaimAmount = disbursementsAndClaimsMaster.ClaimAmount;
                disbursementsAndClaimsMasterDTO.AmountToWallet = disbursementsAndClaimsMaster.AmountToWallet;
                disbursementsAndClaimsMasterDTO.AmountToCredit = disbursementsAndClaimsMaster.AmountToCredit;
                disbursementsAndClaimsMasterDTO.IsSettledAmountCredited = disbursementsAndClaimsMaster.IsSettledAmountCredited ?? false;
                disbursementsAndClaimsMasterDTO.SettledDate = disbursementsAndClaimsMaster.SettledDate != null ? disbursementsAndClaimsMaster.SettledDate.Value.ToShortDateString() : string.Empty;
                disbursementsAndClaimsMasterDTO.SettlementComment = disbursementsAndClaimsMaster.SettlementComment;
                disbursementsAndClaimsMasterDTO.SettlementAccount = disbursementsAndClaimsMaster.SettlementAccount;
                disbursementsAndClaimsMasterDTO.SettlementBankCard = disbursementsAndClaimsMaster.SettlementBankCard;
                disbursementsAndClaimsMasterDTO.AdditionalData = disbursementsAndClaimsMaster.AdditionalData;
                disbursementsAndClaimsMasterDTO.CostCenterId = disbursementsAndClaimsMaster.CostCenterId;
                disbursementsAndClaimsMasterDTO.ApprovalStatusId = disbursementsAndClaimsMaster.ApprovalStatusId;
                disbursementsAndClaimsMasterDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(disbursementsAndClaimsMaster.ApprovalStatusId).Status;


                ListDisbursementsAndClaimsMasterDTO.Add(disbursementsAndClaimsMasterDTO);

            }

            return Ok(ListDisbursementsAndClaimsMasterDTO);
        }

        // GET: api/DisbursementsAndClaimsMasters
        [HttpPost]
        [ActionName("AccountsPayableReport")]
        public async Task<ActionResult<IEnumerable<DisbursementsAndClaimsMasterDTO>>> AccountsPayableReport(AccountsPayableSearchModel searchModel)
        {


            List<DisbursementsAndClaimsMaster> result = new();
            List<DisbursementsAndClaimsMasterDTO> ListDisbursementsAndClaimsMasterDTO = new();

            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<DisbursementsAndClaimsMaster>();


            if (searchModel.IsAccountSettled == true || searchModel.IsAccountSettled == false)
            {
                predicate = predicate.And(x => x.IsSettledAmountCredited == searchModel.IsAccountSettled);
            }


            if (searchModel.SettledAccountsFrom.HasValue)
                predicate = predicate.And(x => x.RecordDate >= searchModel.SettledAccountsFrom);
            if (searchModel.SettledAccountsTo.HasValue)
                predicate = predicate.And(x => x.RecordDate <= searchModel.SettledAccountsTo);

            predicate = predicate.And(x => x.ApprovalStatusId == (int)EApprovalStatus.Approved);

            if (predicate.IsStarted)
            {
                result = _context.DisbursementsAndClaimsMasters.Where(predicate).OrderByDescending(x => x.RecordDate).ToList();
            }
            else
            {
                result = _context.DisbursementsAndClaimsMasters.OrderByDescending(x => x.RecordDate).ToList();
            }


            foreach (DisbursementsAndClaimsMaster disbursementsAndClaimsMaster in result)
            {
                DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO = new();

                disbursementsAndClaimsMasterDTO.Id = disbursementsAndClaimsMaster.Id;
                disbursementsAndClaimsMasterDTO.EmployeeId = disbursementsAndClaimsMaster.EmployeeId;
                disbursementsAndClaimsMasterDTO.EmployeeName = _context.Employees.Find(disbursementsAndClaimsMaster.EmployeeId).GetFullName();
                disbursementsAndClaimsMasterDTO.BlendedRequestId = disbursementsAndClaimsMaster.BlendedRequestId;


                disbursementsAndClaimsMasterDTO.BusinessTypeId = disbursementsAndClaimsMaster.BusinessTypeId;
                disbursementsAndClaimsMasterDTO.BusinessType = disbursementsAndClaimsMasterDTO.BusinessTypeId != null ? _context.BusinessTypes.Find(disbursementsAndClaimsMaster.BusinessTypeId).BusinessTypeName : null;
                disbursementsAndClaimsMasterDTO.BusinessUnitId = disbursementsAndClaimsMaster.BusinessUnitId;
                disbursementsAndClaimsMasterDTO.BusinessUnit = disbursementsAndClaimsMaster.BusinessUnitId != null ? _context.BusinessUnits.Find(disbursementsAndClaimsMaster.BusinessUnitId).GetBusinessUnitName() : null;

                disbursementsAndClaimsMasterDTO.ProjectId = disbursementsAndClaimsMaster.ProjectId;
                disbursementsAndClaimsMasterDTO.ProjectName = disbursementsAndClaimsMaster.ProjectId != null ? _context.Projects.Find(disbursementsAndClaimsMaster.ProjectId).ProjectName : string.Empty;
                disbursementsAndClaimsMasterDTO.SubProjectId = disbursementsAndClaimsMaster.SubProjectId;
                disbursementsAndClaimsMasterDTO.SubProjectName = disbursementsAndClaimsMaster.SubProjectId != null ? _context.SubProjects.Find(disbursementsAndClaimsMaster.SubProjectId).SubProjectName : string.Empty;
                disbursementsAndClaimsMasterDTO.WorkTaskId = disbursementsAndClaimsMaster.WorkTaskId;
                disbursementsAndClaimsMasterDTO.WorkTaskName = disbursementsAndClaimsMaster.WorkTaskId != null ? _context.WorkTasks.Find(disbursementsAndClaimsMaster.WorkTaskId).TaskName : string.Empty;
                disbursementsAndClaimsMasterDTO.CurrencyTypeId = disbursementsAndClaimsMaster.CurrencyTypeId;
                disbursementsAndClaimsMasterDTO.CurrencyType = _context.CurrencyTypes.Find(disbursementsAndClaimsMaster.CurrencyTypeId).CurrencyCode;
                disbursementsAndClaimsMasterDTO.RequestDate = disbursementsAndClaimsMaster.RecordDate;
                disbursementsAndClaimsMasterDTO.ClaimAmount = disbursementsAndClaimsMaster.ClaimAmount;
                disbursementsAndClaimsMasterDTO.AmountToWallet = disbursementsAndClaimsMaster.AmountToWallet;
                disbursementsAndClaimsMasterDTO.AmountToCredit = disbursementsAndClaimsMaster.AmountToCredit;
                disbursementsAndClaimsMasterDTO.IsSettledAmountCredited = disbursementsAndClaimsMaster.IsSettledAmountCredited ?? false;
                disbursementsAndClaimsMasterDTO.SettledDate = disbursementsAndClaimsMaster.SettledDate != null ? disbursementsAndClaimsMaster.SettledDate.Value.ToShortDateString() : string.Empty;
                disbursementsAndClaimsMasterDTO.SettlementComment = disbursementsAndClaimsMaster.SettlementComment;
                disbursementsAndClaimsMasterDTO.SettlementAccount = disbursementsAndClaimsMaster.SettlementAccount;
                disbursementsAndClaimsMasterDTO.SettlementBankCard = disbursementsAndClaimsMaster.SettlementBankCard;
                disbursementsAndClaimsMasterDTO.AdditionalData = disbursementsAndClaimsMaster.AdditionalData;
                disbursementsAndClaimsMasterDTO.CostCenterId = disbursementsAndClaimsMaster.CostCenterId;
                disbursementsAndClaimsMasterDTO.ApprovalStatusId = disbursementsAndClaimsMaster.ApprovalStatusId;
                disbursementsAndClaimsMasterDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(disbursementsAndClaimsMaster.ApprovalStatusId).Status;


                ListDisbursementsAndClaimsMasterDTO.Add(disbursementsAndClaimsMasterDTO);

            }


            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[21]
                {
                    new DataColumn("Id", typeof(int)),
                    new DataColumn("EmployeeName", typeof(string)),
                    new DataColumn("BlendedRequestId", typeof(int)),
                    new DataColumn("Business Type",typeof(string)),
                    new DataColumn("BusinessUnit(s)",typeof(string)),
                    new DataColumn("Project",typeof(string)),
                    new DataColumn("SubProject", typeof(string)),
                    new DataColumn("WorkTask",typeof(string)),
                    new DataColumn("RecordDate",typeof(string)),
                    new DataColumn("CurrencyType",typeof(string)),
                    new DataColumn("ClaimAmount", typeof(Double)),
                    new DataColumn("AmountToWallet", typeof(Double)),
                    new DataColumn("AmountToCredit", typeof(Double)),
                    new DataColumn("CostCenter", typeof(string)),
                    new DataColumn("ApprovalStatus", typeof(string)),
                    new DataColumn("IsSettledAmountCredited", typeof(bool)),
                    new DataColumn("SettledDate", typeof(string)),
                    new DataColumn("SettlementComment", typeof(string)),
                    new DataColumn("SettlementAccount", typeof(string)),
                    new DataColumn("SettlementBankCard", typeof(string)),
                    new DataColumn("AdditionalData", typeof(string))
                });


            foreach (var disbItem in ListDisbursementsAndClaimsMasterDTO)
            {
                dt.Rows.Add(
                     disbItem.Id,
                    disbItem.EmployeeName,
                    disbItem.BlendedRequestId,
                    disbItem.BusinessType,
                    disbItem.BusinessUnit,
                    disbItem.ProjectName,
                    disbItem.SubProjectName,
                    disbItem.WorkTaskName,
                    disbItem.RequestDate,
                    disbItem.CurrencyType,
                    disbItem.ClaimAmount,
                    disbItem.AmountToWallet,
                    disbItem.AmountToCredit,
                    disbItem.CostCenter,
                    disbItem.ApprovalStatusType,
                    disbItem.IsSettledAmountCredited,
                    disbItem.SettledDate,
                    disbItem.SettlementComment,
                    disbItem.SettlementAccount,
                    disbItem.SettlementBankCard,
                    disbItem.AdditionalData
                    );
            }
            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook

            List<string> docUrls = new();
            var docUrl = GetExcel("AccountsPayableReports", dt);

            docUrls.Add(docUrl);

            return Ok(docUrls);
        }



        [HttpPost]
        [ActionName("ExpenseSubClaimsData")]
        public async Task<ActionResult<IEnumerable<ExpenseSubClaimDTO>>> ExpenseSubClaimsData(ExpenseSubClaimsSearchModel searchModel)
        {
            int? empid = searchModel.LoggedEmpId;
            int? reporteeEmpId = searchModel.ReporteeEmpId;


            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<ExpenseSubClaim>();

            if (empid == null || empid == 0)
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
            }

            // all employees who report under the manager
            //if Admin then show all the employee reports irrespective of the Business Unit.
            string empEmailId = _context.Employees.Find(empid).Email;
            var user = await userManager.FindByEmailAsync(empEmailId);
            bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");

            List<ExpenseSubClaim> result = new();
            List<ExpenseSubClaimDTO> ListExpenseSubClaimDTO = new();

            if (searchModel.ExpenseTypeId > 0)
                predicate = predicate.And(x => x.ExpenseTypeId == searchModel.ExpenseTypeId);
            if (searchModel.ExpenseReimbClaimAmountFrom > 0)
                predicate = predicate.And(x => x.ExpenseReimbClaimAmount >= searchModel.ExpenseReimbClaimAmountFrom);
            if (searchModel.ExpenseReimbClaimAmountTo > 0)
                predicate = predicate.And(x => x.ExpenseReimbClaimAmount >= searchModel.ExpenseReimbClaimAmountTo);
            if (searchModel.CostCenterId != null && searchModel.CostCenterId != 0)
                predicate = predicate.And(x => x.CostCenterId != searchModel.CostCenterId);
            if (searchModel.RequestRaisedDateFrom.HasValue)
                predicate = predicate.And(x => x.InvoiceDate <= searchModel.RequestRaisedDateFrom);
            if (searchModel.RequestRaisedDateTo.HasValue)
                predicate = predicate.And(x => x.InvoiceDate <= searchModel.RequestRaisedDateTo);
            //if (searchModel.ApprovalStatusTypeId > 0)
            //    predicate = predicate.And(x => x.ApprovalStatusTypeId == searchModel.ApprovalStatusTypeId);



            if (reporteeEmpId != null && searchModel.IsManager == true)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }
            else if (reporteeEmpId == null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == empid);
            }
            else if (reporteeEmpId != null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }

            if (isAdmin)
            {
                result = predicate.IsStarted ? _context.ExpenseSubClaims.Where(predicate).OrderBy(e => e.Id).ToList() : _context.ExpenseSubClaims.OrderBy(e => e.Id).ToList();

            }
            else if (searchModel.IsManager)
            {
                result = predicate.IsStarted ? _context.ExpenseSubClaims.Where(predicate).OrderBy(e => e.Id).ToList() : _context.ExpenseSubClaims.OrderBy(e => e.Id).ToList();

                List<int> mgrProjects = _context.ProjectManagements.Where(x => x.EmployeeId == empid).Select(p => p.ProjectId).ToList(); //if projManager get projects

                List<int?> mgrLinkedBusinessUnits = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == empid).Select(s => s.BusinessUnitId).Distinct().ToList();
                List<int> mgrReportees = _context.EmployeeExtendedInfos.Where(r => mgrLinkedBusinessUnits.Contains(r.BusinessUnitId)).Select(s => s.EmployeeId).Distinct().ToList();
                result = result.Where(r => mgrReportees.Contains(r.EmployeeId ?? 0) || mgrProjects.Contains(r.ProjectId ?? 0)).OrderBy(e => e.Id).ToList();

                //var mgrDeptId = _context.Employees.Find(empid).DepartmentId;
                //List<int> mgrReportees = _context.Employees.Where(e => e.DepartmentId == mgrDeptId).Select(s => s.Id).ToList();
            }
            else if (!searchModel.IsManager)
            {
                 

                result = _context.ExpenseSubClaims.Where(predicate).OrderBy(e => e.Id).ToList();
            }




            foreach (var expenseSubClaim in result)
            {
                ExpenseSubClaimDTO expenseSubClaimDTO = new();
                //string empName = _context.Employees.Find(expenseSubClaim.EmployeeId).GetFullName();
                //string curCode = _context.CurrencyTypes.Find(expenseSubClaim.CurrencyTypeId).CurrencyCode;


                var expReimReq = await _context.ExpenseReimburseRequests.FindAsync(expenseSubClaim.ExpenseReimburseRequestId);

                expenseSubClaimDTO.ExpenseReimburseRequestId = expenseSubClaim.ExpenseReimburseRequestId;
                expenseSubClaimDTO.Id = expenseSubClaim.Id;
                expenseSubClaimDTO.EmployeeId = expenseSubClaim.EmployeeId;
                expenseSubClaimDTO.EmployeeName = _context.Employees.Find(expenseSubClaim.EmployeeId).GetFullName();
                expenseSubClaimDTO.ExpenseReimbClaimAmount = expenseSubClaim.ExpenseReimbClaimAmount;
                expenseSubClaimDTO.RequestDate = expReimReq.RequestDate;
                expenseSubClaimDTO.InvoiceNo = expenseSubClaim.InvoiceNo;
                expenseSubClaimDTO.InvoiceDate = expenseSubClaim.InvoiceDate;
                expenseSubClaimDTO.Tax = expenseSubClaim.Tax;
                expenseSubClaimDTO.TaxAmount = expenseSubClaim.TaxAmount;


                expenseSubClaimDTO.VendorId = expenseSubClaim.VendorId ?? null;
                expenseSubClaimDTO.AdditionalVendor = expenseSubClaim.VendorId == 0 ? expenseSubClaim.AdditionalVendor : String.Empty;

                if (string.IsNullOrEmpty(expenseSubClaim.AdditionalVendor))
                {
                    expenseSubClaimDTO.Vendor = expenseSubClaim.VendorId != null ? _context.Vendors.Find(expenseSubClaim.VendorId).VendorName : String.Empty;
                }
                else
                {
                    expenseSubClaimDTO.Vendor = expenseSubClaimDTO.AdditionalVendor;
                }

                expenseSubClaimDTO.Location = expenseSubClaim.Location;
                expenseSubClaimDTO.Description = expenseSubClaim.Description;
                expenseSubClaimDTO.CurrencyTypeId = expReimReq.CurrencyTypeId;
                expenseSubClaimDTO.CurrencyType = _context.CurrencyTypes.Find(expReimReq.CurrencyTypeId).CurrencyCode;
                expenseSubClaimDTO.ExpenseTypeId = expenseSubClaim.ExpenseTypeId;
                expenseSubClaimDTO.ExpenseType = _context.ExpenseTypes.Find(expenseSubClaim.ExpenseTypeId).ExpenseTypeName;
                expenseSubClaimDTO.GeneralLedgerId = _context.ExpenseTypes.Find(expenseSubClaim.ExpenseTypeId).GeneralLedgerId;
                expenseSubClaimDTO.GeneralLedger = _context.GeneralLedger.Find(expenseSubClaimDTO.GeneralLedgerId).GeneralLedgerAccountNo + ":" + _context.GeneralLedger.Find(expenseSubClaimDTO.GeneralLedgerId).GeneralLedgerAccountName;

                expenseSubClaimDTO.BusinessTypeId = expenseSubClaim.BusinessTypeId;
                expenseSubClaimDTO.BusinessType = expenseSubClaim.BusinessTypeId != null ? _context.BusinessTypes.Find(expenseSubClaim.BusinessTypeId).BusinessTypeName : null;
                expenseSubClaimDTO.BusinessUnitId = expenseSubClaim.BusinessUnitId;
                expenseSubClaimDTO.BusinessUnit = expenseSubClaim.BusinessUnitId != null ? _context.BusinessUnits.Find(expenseSubClaim.BusinessUnitId).GetBusinessUnitName() : null;



                expenseSubClaimDTO.ProjectId = expenseSubClaim.ProjectId;
                expenseSubClaimDTO.ProjectName = expenseSubClaim.ProjectId != null ? _context.Projects.Find(expenseSubClaim.ProjectId).ProjectName : string.Empty;

                expenseSubClaimDTO.CostCenterId = expenseSubClaim.CostCenterId;
                expenseSubClaimDTO.CostCenter = _context.CostCenters.Find(expenseSubClaim.CostCenterId).GetCostCentre();

                expenseSubClaimDTO.SubProjectId = expenseSubClaim.SubProjectId;
                expenseSubClaimDTO.SubProjectName = expenseSubClaim.SubProjectId != null ? _context.SubProjects.Find(expenseSubClaim.SubProjectId).SubProjectName : string.Empty;

                expenseSubClaimDTO.WorkTaskId = expenseSubClaim.WorkTaskId;
                expenseSubClaimDTO.WorkTaskName = expenseSubClaim.WorkTaskId != null ? _context.WorkTasks.Find(expenseSubClaim.WorkTaskId).TaskName : string.Empty;

                ListExpenseSubClaimDTO.Add(expenseSubClaimDTO);
            }

            return Ok(ListExpenseSubClaimDTO);
        }



        [HttpPost]
        [ActionName("ExpenseSubClaimsReport")]
        public async Task<ActionResult<IEnumerable<ExpenseSubClaimDTO>>> ExpenseSubClaimsReport(ExpenseSubClaimsSearchModel searchModel)
        {
            int? empid = searchModel.LoggedEmpId;
            int? reporteeEmpId = searchModel.ReporteeEmpId;


            //using predicate builder to add multiple filter cireteria
            var predicate = PredicateBuilder.New<ExpenseSubClaim>();

            if (empid == null || empid == 0)
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Employee Id not valid" });
            }

            // all employees who report under the manager
            //if Admin then show all the employee reports irrespective of the Business Unit.
            string empEmailId = _context.Employees.Find(empid).Email;
            var user = await userManager.FindByEmailAsync(empEmailId);
            bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");

            List<ExpenseSubClaim> result = new();
            List<ExpenseSubClaimDTO> ListExpenseSubClaimDTO = new();

            if (searchModel.ExpenseTypeId > 0)
                predicate = predicate.And(x => x.ExpenseTypeId == searchModel.ExpenseTypeId);
            if (searchModel.ExpenseReimbClaimAmountFrom > 0)
                predicate = predicate.And(x => x.ExpenseReimbClaimAmount >= searchModel.ExpenseReimbClaimAmountFrom);
            if (searchModel.ExpenseReimbClaimAmountTo > 0)
                predicate = predicate.And(x => x.ExpenseReimbClaimAmount >= searchModel.ExpenseReimbClaimAmountTo);
            if (searchModel.CostCenterId != null && searchModel.CostCenterId != 0)
                predicate = predicate.And(x => x.CostCenterId != searchModel.CostCenterId);
            if (searchModel.RequestRaisedDateFrom.HasValue)
                predicate = predicate.And(x => x.InvoiceDate <= searchModel.RequestRaisedDateFrom);
            if (searchModel.RequestRaisedDateTo.HasValue)
                predicate = predicate.And(x => x.InvoiceDate <= searchModel.RequestRaisedDateTo);
            //if (searchModel.ApprovalStatusTypeId > 0)
            //    predicate = predicate.And(x => x.ApprovalStatusTypeId == searchModel.ApprovalStatusTypeId);

            if (reporteeEmpId != null && searchModel.IsManager == true)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }
            else if (reporteeEmpId == null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == empid);
            }
            else if (reporteeEmpId != null && searchModel.IsManager == false)
            {
                predicate = predicate.And(x => x.EmployeeId == reporteeEmpId);
            }
            if (isAdmin)
            {
                result = predicate.IsStarted ? _context.ExpenseSubClaims.Where(predicate).OrderBy(e => e.Id).ToList() : _context.ExpenseSubClaims.OrderBy(e => e.Id).ToList();

            }
            else if (searchModel.IsManager)
            {
                result = predicate.IsStarted ? _context.ExpenseSubClaims.Where(predicate).OrderBy(e => e.Id).ToList() : _context.ExpenseSubClaims.OrderBy(e => e.Id).ToList();

                List<int> mgrProjects = _context.ProjectManagements.Where(x => x.EmployeeId == empid).Select(p => p.ProjectId).ToList(); //if projManager get projects

                List<int?> mgrLinkedBusinessUnits = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == empid).Select(s => s.BusinessUnitId).Distinct().ToList();
                List<int> mgrReportees = _context.EmployeeExtendedInfos.Where(r => mgrLinkedBusinessUnits.Contains(r.BusinessUnitId)).Select(s => s.EmployeeId).Distinct().ToList();
                result = result.Where(r => mgrReportees.Contains(r.EmployeeId ?? 0) || mgrProjects.Contains(r.ProjectId ?? 0)).OrderBy(e => e.Id).ToList();

            }
            else if (!searchModel.IsManager)
            {
                

                result = _context.ExpenseSubClaims.Where(predicate).OrderBy(e => e.Id).ToList();
            }




            foreach (var expenseSubClaim in result)
            {
                ExpenseSubClaimDTO expenseSubClaimDTO = new();
                //string empName = _context.Employees.Find(expenseSubClaim.EmployeeId).GetFullName();
                //string curCode = _context.CurrencyTypes.Find(expenseSubClaim.CurrencyTypeId).CurrencyCode;


                var expReimReq = await _context.ExpenseReimburseRequests.FindAsync(expenseSubClaim.ExpenseReimburseRequestId);

                expenseSubClaimDTO.ExpenseReimburseRequestId = expenseSubClaim.ExpenseReimburseRequestId;
                expenseSubClaimDTO.Id = expenseSubClaim.Id;
                expenseSubClaimDTO.EmployeeId = expenseSubClaim.EmployeeId;
                expenseSubClaimDTO.EmployeeName = _context.Employees.Find(expenseSubClaim.EmployeeId).GetFullName();
                expenseSubClaimDTO.ExpenseReimbClaimAmount = expenseSubClaim.ExpenseReimbClaimAmount;
                expenseSubClaimDTO.RequestDate = expReimReq.RequestDate;
                expenseSubClaimDTO.InvoiceNo = expenseSubClaim.InvoiceNo;
                expenseSubClaimDTO.InvoiceDate = expenseSubClaim.InvoiceDate;
                expenseSubClaimDTO.Tax = expenseSubClaim.Tax;
                expenseSubClaimDTO.TaxAmount = expenseSubClaim.TaxAmount;

                if (string.IsNullOrEmpty(expenseSubClaim.AdditionalVendor))
                {
                    expenseSubClaimDTO.Vendor = expenseSubClaim.VendorId != null ? _context.Vendors.Find(expenseSubClaim.VendorId).VendorName : String.Empty;
                }
                else
                {
                    expenseSubClaimDTO.Vendor = expenseSubClaimDTO.AdditionalVendor;
                }
                expenseSubClaimDTO.Location = expenseSubClaim.Location;
                expenseSubClaimDTO.Description = expenseSubClaim.Description;
                expenseSubClaimDTO.CurrencyTypeId = expReimReq.CurrencyTypeId;
                expenseSubClaimDTO.CurrencyType = _context.CurrencyTypes.Find(expReimReq.CurrencyTypeId).CurrencyCode;
                expenseSubClaimDTO.ExpenseTypeId = expenseSubClaim.ExpenseTypeId;
                expenseSubClaimDTO.ExpenseType = _context.ExpenseTypes.Find(expenseSubClaim.ExpenseTypeId).ExpenseTypeName;

                expenseSubClaimDTO.GeneralLedgerId = _context.ExpenseTypes.Find(expenseSubClaim.ExpenseTypeId).GeneralLedgerId;
                expenseSubClaimDTO.GeneralLedger = _context.GeneralLedger.Find(expenseSubClaimDTO.GeneralLedgerId).GeneralLedgerAccountNo + ":" + _context.GeneralLedger.Find(expenseSubClaimDTO.GeneralLedgerId).GeneralLedgerAccountName;


                expenseSubClaimDTO.BusinessTypeId = expenseSubClaim.BusinessTypeId;
                expenseSubClaimDTO.BusinessType = expenseSubClaim.BusinessTypeId != null ? _context.BusinessTypes.Find(expenseSubClaim.BusinessTypeId).BusinessTypeName : null;
                expenseSubClaimDTO.BusinessUnitId = expenseSubClaim.BusinessUnitId;
                expenseSubClaimDTO.BusinessUnit = expenseSubClaim.BusinessUnitId != null ? _context.BusinessUnits.Find(expenseSubClaim.BusinessUnitId).GetBusinessUnitName() : null;

                expenseSubClaimDTO.ProjectId = expenseSubClaim.ProjectId;
                expenseSubClaimDTO.ProjectName = expenseSubClaim.ProjectId != null ? _context.Projects.Find(expenseSubClaim.ProjectId).ProjectName : string.Empty;

                expenseSubClaimDTO.CostCenterId = expenseSubClaim.CostCenterId;
                expenseSubClaimDTO.CostCenter = _context.CostCenters.Find(expenseSubClaim.CostCenterId).GetCostCentre();

                expenseSubClaimDTO.SubProjectId = expenseSubClaim.SubProjectId;
                expenseSubClaimDTO.SubProjectName = expenseSubClaim.SubProjectId != null ? _context.SubProjects.Find(expenseSubClaim.SubProjectId).SubProjectName : string.Empty;

                expenseSubClaimDTO.WorkTaskId = expenseSubClaim.WorkTaskId;
                expenseSubClaimDTO.WorkTaskName = expenseSubClaim.WorkTaskId != null ? _context.WorkTasks.Find(expenseSubClaim.WorkTaskId).TaskName : string.Empty;

                ListExpenseSubClaimDTO.Add(expenseSubClaimDTO);
            }


            DataTable dt = new();
            dt.Columns.AddRange(new DataColumn[21]
                {
                    //new DataColumn("Id", typeof(int)),
                    new DataColumn("ExpenseReimburseId", typeof(int)),
                    new DataColumn("ExpenseSubClaimId", typeof(int)),
                    new DataColumn("EmployeeName", typeof(string)),
                    new DataColumn("RequestDate",typeof(string)),
                    new DataColumn("InvoiceNo",typeof(string)),
                    new DataColumn("InvoiceDate",typeof(string)),
                    new DataColumn("Tax",typeof(float)),
                    new DataColumn("TaxAmount",typeof(Double)),
                    new DataColumn("ExpSubClaimAmount",typeof(Double)),
                    new DataColumn("Vendor",typeof(string)),
                    new DataColumn("Location",typeof(string)),
                    new DataColumn("CurrencyType",typeof(string)),
                    new DataColumn("ExpenseType", typeof(string)),
                    new DataColumn("BusinessType", typeof(string)),
                    new DataColumn("BusinessUnit(s)", typeof(string)),
                    new DataColumn("GeneralLedger", typeof(string)),
                    new DataColumn("CostCenter",typeof(string)),
                    new DataColumn("Project",typeof(string)),
                    new DataColumn("SubProject", typeof(string)),
                    new DataColumn("WorkTask",typeof(string)),
                    new DataColumn("Description",typeof(string))
                });


            foreach (var claimItem in ListExpenseSubClaimDTO)
            {
                dt.Rows.Add(
                    claimItem.ExpenseReimburseRequestId,
                    claimItem.Id,
                    claimItem.EmployeeName,
                    claimItem.RequestDate,
                    claimItem.InvoiceNo,
                    claimItem.InvoiceDate,
                    claimItem.Tax,
                    claimItem.TaxAmount,
                    claimItem.ExpenseReimbClaimAmount,
                    claimItem.Vendor,
                    claimItem.Location,
                    claimItem.CurrencyType,
                    claimItem.ExpenseType,
                    claimItem.BusinessType,
                    claimItem.BusinessUnit,
                    claimItem.GeneralLedger,
                    claimItem.CostCenter,
                    claimItem.ProjectName,
                    claimItem.SubProjectName,
                    claimItem.WorkTaskName,
                    claimItem.Description
                    );
            }
            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook

            List<string> docUrls = new();
            var docUrl = GetExcel("ExpenseSubClaimsReports", dt);

            docUrls.Add(docUrl);

            return Ok(docUrls);
        }



        private string GetExcel(string reporttype, DataTable dt)
        {
            // Creating the Excel workbook 
            // Add the datatable to the Excel workbook
            using XLWorkbook wb = new();
            wb.Worksheets.Add(dt, reporttype);

            string xlfileName = reporttype + "_" + Guid.NewGuid().ToString() + ".xlsx";
            //string xlfileName = reporttype + ".xlsx";

            using MemoryStream stream = new();

            wb.SaveAs(stream);

            //string uploadsfolder = Path.Combine(hostingEnvironment.ContentRootPath, "Images");
            string uploadsfolder = Path.Combine(hostingEnvironment.ContentRootPath, "Reportdocs");

            string filepath = Path.Combine(uploadsfolder, xlfileName);

            string[] fileswithpath = Directory.GetFiles(uploadsfolder);
            foreach (string file in fileswithpath)
            {
                System.IO.File.Delete(file);
            }

            //if (System.IO.File.Exists(filepath))
            //    System.IO.File.Delete(filepath);

            using var outputtream = new FileStream(filepath, FileMode.Create);

            wb.SaveAs(outputtream);

            string docurl = Directory.EnumerateFiles(uploadsfolder).Select(f => filepath).FirstOrDefault().ToString();

            // return File(stream.ToArray(), "Application/Ms-Excel", xlfileName);

            return docurl;

        }


        private string getJobRolesByEmployeeId(int empId)
        {
            var emp = _context.Employees.FindAsync(empId);
            var jobRoleIds = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == empId).Select(s => s.JobRoleId).ToList();

            List<string> listJobRolesInString = new List<string>();

            foreach (int jobroleId in jobRoleIds)
            {
                string tmpJobrole = _context.JobRoles.Find(jobroleId).GetJobRole();
                if (tmpJobrole != String.Empty)
                {
                    listJobRolesInString.Add(tmpJobrole);
                }

            }

            return (String.Join(';', listJobRolesInString));
        }


        private string getBusinessUnitsByEmployeeId(int empId)
        {
            var emp = _context.Employees.FindAsync(empId);
            var listBusinessUnitIds = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == empId).Select(s => s.BusinessUnitId).ToList();

            List<string> listBusinessUnitsInString = new List<string>();

            foreach (int businessUnitId in listBusinessUnitIds)
            {
                string tmpbusinessUnitName = _context.BusinessUnits.Find(businessUnitId).BusinessUnitName;
                if (tmpbusinessUnitName != String.Empty)
                {
                    listBusinessUnitsInString.Add(tmpbusinessUnitName);
                }

            }

            return (String.Join(';', listBusinessUnitsInString));
        }

        private string getApprovalGroupsByEmployeeId(int empId)
        {
            var emp = _context.Employees.FindAsync(empId);
            var listApprovalGroupIds = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == empId).Select(s => s.ApprovalGroupId).ToList();

            List<string> listApprovalGroupsInString = new List<string>();

            foreach (int approvalGrpId in listApprovalGroupIds)
            {
                string tmpbApprovalGrpName = _context.ApprovalGroups.Find(approvalGrpId).ApprovalGroupCode;
                if (tmpbApprovalGrpName != String.Empty)
                {
                    listApprovalGroupsInString.Add(tmpbApprovalGrpName);
                }

            }

            return (String.Join(';', listApprovalGroupsInString));
        }

        /////End of methods
        ///

    }
}

