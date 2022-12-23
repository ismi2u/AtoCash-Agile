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

namespace AtoCashAPI.Controllers.BasicControls
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeeExtendedInfoController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public EmployeeExtendedInfoController(AtoCashDbContext context)
        {
            _context = context;
        }

        // GET: api/EmployeeExtendedInfo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeExtendedInfoDTO>>> GetEmployeeExtendedInfos()
        {
            var employeeExtendedInfos = await _context.EmployeeExtendedInfos.ToListAsync();

            List<EmployeeExtendedInfoDTO> ListEmployeeExtendedInfoDTOs = new();

            foreach (EmployeeExtendedInfo employeeExtendedInfo in employeeExtendedInfos)
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

            return ListEmployeeExtendedInfoDTOs;
        }


        // GET: api/EmployeeExtendedInfo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeExtendedInfoDTO>> GetEmployeeExtendedInfoById(int? id)
        {

            var employeeExtendedInfo = await _context.EmployeeExtendedInfos.FindAsync(id);

            if (employeeExtendedInfo == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id invalid!" });
            }

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

            return empExtendedInfoDTO;
        }


        // GET: api/EmployeeExtendedInfo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<EmployeeExtendedInfoDTO>>> GetEmployeeExtendedInfoByEmployeeId(int? id)
        {

            var ListExtendedInfoForEmployee = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == id).ToList();

            List<EmployeeExtendedInfoDTO> ListEmployeeExtendedInfoDTOs = new();

            foreach (EmployeeExtendedInfo employeeExtendedInfo in ListExtendedInfoForEmployee)
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

            return ListEmployeeExtendedInfoDTOs;
        }

        // PUT: api/EmployeeExtendedInfo/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployeeExtendedInfo(int? id, EmployeeExtendedInfoDTO employeeExtendedInfoDTO)
        {
            if (id != employeeExtendedInfoDTO.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "EmployeeExtendedInfo Id is invalid" });
            }

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {

                var empExtendedInfo = await _context.EmployeeExtendedInfos.FindAsync(id);

                if (empExtendedInfo == null)
                {
                    return Conflict(new RespStatus { Status = "Failure", Message = "EmployeeExtendedInfo Id object is null" });
                }
                else
                {
                    try
                    {
                        empExtendedInfo.BusinessTypeId = employeeExtendedInfoDTO.BusinessTypeId;
                        empExtendedInfo.BusinessUnitId = employeeExtendedInfoDTO.BusinessUnitId;
                        empExtendedInfo.EmployeeId = employeeExtendedInfoDTO.EmployeeId;
                        empExtendedInfo.JobRoleId = employeeExtendedInfoDTO.JobRoleId;
                        empExtendedInfo.ApprovalGroupId = employeeExtendedInfoDTO.ApprovalGroupId;
                        empExtendedInfo.StatusTypeId = employeeExtendedInfoDTO.StatusTypeId;

                        EmpCurrentPettyCashBalance currentEmpPettyCashBalance = _context.EmpCurrentPettyCashBalances.Where(b => b.EmployeeId == employeeExtendedInfoDTO.EmployeeId).FirstOrDefault();

                        double oldJobRoleLimit = _context.JobRoles.Find(empExtendedInfo.JobRoleId).MaxPettyCashAllowed ?? 0;
                        double NewJobRoleLimit = _context.JobRoles.Find(employeeExtendedInfoDTO.JobRoleId).MaxPettyCashAllowed ?? 0;

                        string strPettyCashLimits = currentEmpPettyCashBalance.AllPettyCashLimits;

                        currentEmpPettyCashBalance.AllPettyCashLimits = RemoveStringMaxLimits(strPettyCashLimits, oldJobRoleLimit); //REMOVE OLD LIMIT
                        currentEmpPettyCashBalance.AllPettyCashLimits = AddStringMaxLimits(strPettyCashLimits, NewJobRoleLimit); // ADD NEW LIMIT
                        currentEmpPettyCashBalance.MaxPettyCashLimit = GetMaxFromStringDoubles(currentEmpPettyCashBalance.AllPettyCashLimits);

                        _context.EmpCurrentPettyCashBalances.Update(currentEmpPettyCashBalance);

                        _context.EmployeeExtendedInfos.Update(empExtendedInfo);

                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return Conflict(new RespStatus { Status = "Failure", Message = "EmployeeExtendedInfo cannot be updated!" });
                    }
                }

                await AtoCashDbContextTransaction.CommitAsync();
            }
            return Ok(new RespStatus { Status = "Success", Message = "EmployeeExtendedInfo Records Updated!" });
        }


        // POST: api/EmployeeExtendedInfo
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EmployeeExtendedInfo>> PostEmployeeExtendedInfo(EmployeeExtendedInfoDTO employeeExtendedInfoDTO)
        {

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    EmployeeExtendedInfo empExtendedInfo = new EmployeeExtendedInfo();

                    empExtendedInfo.BusinessTypeId = employeeExtendedInfoDTO.BusinessTypeId;
                    empExtendedInfo.BusinessUnitId = employeeExtendedInfoDTO.BusinessUnitId;
                    empExtendedInfo.EmployeeId = employeeExtendedInfoDTO.EmployeeId;
                    empExtendedInfo.JobRoleId = employeeExtendedInfoDTO.JobRoleId;
                    empExtendedInfo.ApprovalGroupId = employeeExtendedInfoDTO.ApprovalGroupId;
                    empExtendedInfo.StatusTypeId = employeeExtendedInfoDTO.StatusTypeId;


                    EmpCurrentPettyCashBalance currentEmpPettyCashBalance = _context.EmpCurrentPettyCashBalances.Where(b => b.EmployeeId == employeeExtendedInfoDTO.EmployeeId).FirstOrDefault();

                    string strPettyCashLimits = currentEmpPettyCashBalance.AllPettyCashLimits;
                    double JobRoleLimit = _context.JobRoles.Find(employeeExtendedInfoDTO.JobRoleId).MaxPettyCashAllowed ?? 0;

                    currentEmpPettyCashBalance.AllPettyCashLimits = AddStringMaxLimits(strPettyCashLimits, JobRoleLimit);
                    currentEmpPettyCashBalance.MaxPettyCashLimit = GetMaxFromStringDoubles(currentEmpPettyCashBalance.AllPettyCashLimits);

                    //this is problematics needs resolution
                    //this is problematics needs resolution

                    double? diffamount = currentEmpPettyCashBalance.MaxPettyCashLimit - currentEmpPettyCashBalance.CurBalance;

                    if (currentEmpPettyCashBalance.CurBalance == 0)
                    {
                        currentEmpPettyCashBalance.CurBalance = currentEmpPettyCashBalance.MaxPettyCashLimit;
                    }
                    else if (currentEmpPettyCashBalance.CurBalance > 0)
                    {
                        currentEmpPettyCashBalance.CurBalance = currentEmpPettyCashBalance.CurBalance + diffamount;
                    }
                    else //(currentEmpPettyCashBalance.CurBalance < 0)
                    {
                        currentEmpPettyCashBalance.CurBalance = currentEmpPettyCashBalance.CurBalance + diffamount;
                    }

                    currentEmpPettyCashBalance.UpdatedOn = DateTime.UtcNow;

                    _context.EmpCurrentPettyCashBalances.Update(currentEmpPettyCashBalance);

                    _context.EmployeeExtendedInfos.Add(empExtendedInfo);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Conflict(new RespStatus { Status = "Failure", Message = "EmployeeExtendedInfo cannot be updated!" });
                }



                await AtoCashDbContextTransaction.CommitAsync();
            }


            return Ok(new RespStatus { Status = "Success", Message = "Employee EmployeeExtendedInfo Recorded!" });
        }

        // DELETE: api/EmployeeExtendedInfo/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeExtendedInfo(int? id)
        {
            var employeeExtendedInfo = await _context.EmployeeExtendedInfos.FindAsync(id);

            var empId = employeeExtendedInfo.EmployeeId;

            if (employeeExtendedInfo == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "EmployeeExtendedInfo Id is invalid!" });
            }




            bool blnUsedInTravelReq = false; // remove this after travelrequest is fixed below
            bool blnUsedInExpeReimReq = false; // remove this after ExpReimburse is fixed below

            // bool blnUsedInTravelReq = _context.TravelApprovalRequests.Where(p => p.EmployeeId == employeeExtendedInfo.EmployeeId && p.BusinessUnitId == employeeExtendedInfo.BusinessUnitId).Any();
            bool blnUsedInCashAdvReq = _context.PettyCashRequests.Where(p => p.EmployeeId == employeeExtendedInfo.EmployeeId && p.BusinessUnitId == employeeExtendedInfo.BusinessUnitId).Any();
            // bool blnUsedInExpeReimReq = _context.ExpenseReimburseRequests.Where(p => p.EmployeeId == employeeExtendedInfo.EmployeeId && p.BusinessUnitId == employeeExtendedInfo.BusinessUnitId).Any();

            if (blnUsedInTravelReq || blnUsedInCashAdvReq || blnUsedInExpeReimReq)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee in Use, Cant delete!" });
            }

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {


                EmpCurrentPettyCashBalance currentEmpPettyCashBalance = _context.EmpCurrentPettyCashBalances.Where(b => b.EmployeeId == employeeExtendedInfo.EmployeeId).FirstOrDefault();

                double JobRoleLimit = _context.JobRoles.Find(employeeExtendedInfo.JobRoleId).MaxPettyCashAllowed ?? 0;
                string strPettyCashLimits = currentEmpPettyCashBalance.AllPettyCashLimits;

                currentEmpPettyCashBalance.AllPettyCashLimits = RemoveStringMaxLimits(strPettyCashLimits, JobRoleLimit); //REMOVE OLD LIMIT
                currentEmpPettyCashBalance.MaxPettyCashLimit = GetMaxFromStringDoubles(currentEmpPettyCashBalance.AllPettyCashLimits);

                _context.EmpCurrentPettyCashBalances.Update(currentEmpPettyCashBalance);


                _context.EmployeeExtendedInfos.Remove(employeeExtendedInfo);
                await _context.SaveChangesAsync();
                await AtoCashDbContextTransaction.CommitAsync();
            }

            return Ok(new RespStatus { Status = "Success", Message = "EmployeeExtendedInfo Deleted!" });
        }




        private double GetMaxFromStringDoubles(string DoublesInStrings)
        {
            double maximumLimit = 0;

            double[] Limits = DoublesInStrings.Split(';').Select(double.Parse).ToArray();
            maximumLimit = Limits.Max();

            return maximumLimit;
        }


        private string RemoveStringMaxLimits(string MaxLimitsInStrings, double limitAmount)
        {

            string strLimitAmount = string.Format("{0:N2}", Math.Truncate(limitAmount * 100) / 100);

            List<string> ListOfMaxlimits = MaxLimitsInStrings.Split(";").ToList();
            ListOfMaxlimits = ListOfMaxlimits.Where(l => l != strLimitAmount).ToList();

            return string.Join(";", ListOfMaxlimits);
        }


        private string AddStringMaxLimits(string MaxLimitsInStrings, double limitAmount)
        {
            string strLimitAmount = string.Format("{0:N2}", Math.Truncate(limitAmount * 100) / 100);

            List<string> ListOfMaxlimits = new List<string>();

            if (!String.IsNullOrEmpty(MaxLimitsInStrings))
            {
                ListOfMaxlimits = MaxLimitsInStrings.Split(";").ToList();
            }

            ListOfMaxlimits.Add(strLimitAmount);
            return string.Join(";", ListOfMaxlimits);
        }
    }
}
