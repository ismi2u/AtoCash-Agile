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
    [Route("api/[controller]")]
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
                empExtendedInfoDTO.JobRoleId = employeeExtendedInfo.JobRoleId;
                empExtendedInfoDTO.ApprovalGroupId = employeeExtendedInfo.ApprovalGroupId;
                empExtendedInfoDTO.ApprovalLevelId = employeeExtendedInfo.ApprovalLevelId;
                empExtendedInfoDTO.StatusTypeId = employeeExtendedInfo.StatusTypeId;

                ListEmployeeExtendedInfoDTOs.Add(empExtendedInfoDTO);

            }

            return ListEmployeeExtendedInfoDTOs;
        }


        // GET: api/EmployeeExtendedInfo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeExtendedInfoDTO>> GetEmployeeExtendedInfo(int? id)
        {

            var employeeExtendedInfo = await _context.EmployeeExtendedInfos.FindAsync(id);

            if (employeeExtendedInfo == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id invalid!" });
            }

            EmployeeExtendedInfoDTO empExtendedInfoDTO = new();

            empExtendedInfoDTO.Id = employeeExtendedInfo.Id;
            empExtendedInfoDTO.EmployeeId = employeeExtendedInfo.EmployeeId;
            empExtendedInfoDTO.JobRoleId = employeeExtendedInfo.JobRoleId;
            empExtendedInfoDTO.ApprovalGroupId = employeeExtendedInfo.ApprovalGroupId;
            empExtendedInfoDTO.ApprovalLevelId = employeeExtendedInfo.ApprovalLevelId;
            empExtendedInfoDTO.StatusTypeId = employeeExtendedInfo.StatusTypeId;


            return empExtendedInfoDTO;
        }


        // GET: api/EmployeeExtendedInfo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<EmployeeExtendedInfoDTO>>> GetEmployeeExtendedInfoByEmployeeId(int? id)
        {

            var ListExtendedInfoForEmployee =  _context.EmployeeExtendedInfos.Where(e=> e.EmployeeId == id).ToList();
     
            List<EmployeeExtendedInfoDTO> ListEmployeeExtendedInfoDTOs = new();

            foreach (EmployeeExtendedInfo employeeExtendedInfo in ListExtendedInfoForEmployee)
            {
                EmployeeExtendedInfoDTO empExtendedInfoDTO = new();

                empExtendedInfoDTO.Id = employeeExtendedInfo.Id;
                empExtendedInfoDTO.EmployeeId = employeeExtendedInfo.EmployeeId;
                empExtendedInfoDTO.JobRoleId = employeeExtendedInfo.JobRoleId;
                empExtendedInfoDTO.ApprovalGroupId = employeeExtendedInfo.ApprovalGroupId;
                empExtendedInfoDTO.ApprovalLevelId = employeeExtendedInfo.ApprovalLevelId;
                empExtendedInfoDTO.StatusTypeId = employeeExtendedInfo.StatusTypeId;

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


            var empExtendedInfo = await _context.EmployeeExtendedInfos.FindAsync(id);

            if (empExtendedInfo == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "EmployeeExtendedInfo Id object is null" });
            }
            else
            {
                try
                {
                    empExtendedInfo.EmployeeId = employeeExtendedInfoDTO.EmployeeId;
                    empExtendedInfo.JobRoleId = employeeExtendedInfoDTO.JobRoleId;
                    empExtendedInfo.ApprovalGroupId = employeeExtendedInfoDTO.ApprovalGroupId;
                    empExtendedInfo.ApprovalLevelId = employeeExtendedInfoDTO.ApprovalLevelId;
                    empExtendedInfo.StatusTypeId = employeeExtendedInfoDTO.StatusTypeId;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Conflict(new RespStatus { Status = "Failure", Message = "EmployeeExtendedInfo cannot be updated!" });
                }
            }
            return Ok(new RespStatus { Status = "Success", Message = "EmployeeExtendedInfo Records Updated!" });
        }
    

        // POST: api/EmployeeExtendedInfo
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EmployeeExtendedInfo>> PostEmployeeExtendedInfo(EmployeeExtendedInfoDTO employeeExtendedInfoDTO)
        {
            try
            {
                EmployeeExtendedInfo empExtendedInfo = new EmployeeExtendedInfo();

                empExtendedInfo.EmployeeId = employeeExtendedInfoDTO.EmployeeId;
                empExtendedInfo.JobRoleId = employeeExtendedInfoDTO.JobRoleId;
                empExtendedInfo.ApprovalGroupId = employeeExtendedInfoDTO.ApprovalGroupId;
                empExtendedInfo.ApprovalLevelId = employeeExtendedInfoDTO.ApprovalLevelId;
                empExtendedInfo.StatusTypeId = employeeExtendedInfoDTO.StatusTypeId;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "EmployeeExtendedInfo cannot be updated!" });
            }

            return Ok(new RespStatus { Status = "Success", Message = "Employee EmployeeExtendedInfo Recorded!" });
        }

        // DELETE: api/EmployeeExtendedInfo/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeExtendedInfo(int? id)
        {
            var employeeExtendedInfo = await _context.EmployeeExtendedInfos.FindAsync(id);

            if (employeeExtendedInfo == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "EmployeeExtendedInfo Id is invalid!" });
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
                _context.EmployeeExtendedInfos.Remove(employeeExtendedInfo);
                var empPettyCashBal = _context.EmpCurrentPettyCashBalances.Where(e => e.EmployeeId == id).FirstOrDefault();
                await _context.SaveChangesAsync();
                await AtoCashDbContextTransaction.CommitAsync();
            }

            return Ok(new RespStatus { Status = "Success", Message = "EmployeeExtendedInfo Deleted!" });
        }

    
    }
}
