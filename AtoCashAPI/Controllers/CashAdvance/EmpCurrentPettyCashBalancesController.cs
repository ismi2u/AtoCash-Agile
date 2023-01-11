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
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User")]
    public class EmpCurrentCashAdvanceBalancesController : ControllerBase
    {
        private readonly AtoCashDbContext _context;

        public EmpCurrentCashAdvanceBalancesController(AtoCashDbContext context)
        {
            _context = context;
        }

        // GET: api/EmpCurrentCashAdvanceBalances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpCurrentCashAdvanceBalanceDTO>>> GetEmpCurrentCashAdvanceBalances()
        {
            List<EmpCurrentCashAdvanceBalanceDTO> ListEmpCurrentCashAdvanceBalanceDTO = new();

            var EmpCurrentCashAdvanceBalances = await _context.EmpCurrentCashAdvanceBalances.ToListAsync();

            foreach (EmpCurrentCashAdvanceBalance EmpCurrentCashAdvanceBalance in EmpCurrentCashAdvanceBalances)
            {
                EmpCurrentCashAdvanceBalanceDTO EmpCurrentCashAdvanceBalanceDTO = new()
                {
                    Id = EmpCurrentCashAdvanceBalance.Id,
                    EmployeeId = EmpCurrentCashAdvanceBalance.EmployeeId,
                    CurBalance = EmpCurrentCashAdvanceBalance.CurBalance,
                    UpdatedOn = EmpCurrentCashAdvanceBalance.UpdatedOn
                };
                ListEmpCurrentCashAdvanceBalanceDTO.Add(EmpCurrentCashAdvanceBalanceDTO);

            }

            return ListEmpCurrentCashAdvanceBalanceDTO;
        }

        // GET: api/EmpCurrentCashAdvanceBalances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmpCurrentCashAdvanceBalanceDTO>> GetEmpCurrentCashAdvanceBalance(int id)
        {
            EmpCurrentCashAdvanceBalanceDTO EmpCurrentCashAdvanceBalanceDTO = new();

            var EmpCurrentCashAdvanceBalance = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == id).FirstOrDefault();


            if (EmpCurrentCashAdvanceBalance == null)
            {
                EmpCurrentCashAdvanceBalanceDTO.CurBalance = 0;
                return EmpCurrentCashAdvanceBalanceDTO;
            }

            await Task.Run(() =>
            {
                //EmpCurrentCashAdvanceBalanceDTO.Id = EmpCurrentCashAdvanceBalance.Id;
                EmpCurrentCashAdvanceBalanceDTO.EmployeeId = EmpCurrentCashAdvanceBalance.EmployeeId;
                EmpCurrentCashAdvanceBalanceDTO.CurBalance = EmpCurrentCashAdvanceBalance.CurBalance;
                EmpCurrentCashAdvanceBalanceDTO.UpdatedOn = EmpCurrentCashAdvanceBalance.UpdatedOn;
            });



            return EmpCurrentCashAdvanceBalanceDTO;
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<EmpAllCurBalStatusDTO>> GetEmpMaxlimitCurBalAndCashInHandStatus(int id)
        {
            EmpAllCurBalStatusDTO empAllCurBalStatusDTO = new();

            var EmpCurrentCashAdvanceBalance = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == id).FirstOrDefault();

            if (EmpCurrentCashAdvanceBalance == null)
            {
                empAllCurBalStatusDTO.CurBalance = 0;
            }

            await Task.Run(() =>
            {
                empAllCurBalStatusDTO.CurBalance = EmpCurrentCashAdvanceBalance.CurBalance;
                empAllCurBalStatusDTO.MaxLimit = EmpCurrentCashAdvanceBalance.MaxCashAdvanceLimit;


                //condition to check  the cash-in-hand and set it to Zero '0'
                EmpCurrentCashAdvanceBalance empCurPettyBalance = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == id).FirstOrDefault();

                empAllCurBalStatusDTO.CashInHand = empCurPettyBalance.CashOnHand;

                empAllCurBalStatusDTO.CurBalance = empCurPettyBalance.CurBalance;


                empAllCurBalStatusDTO.PendingSettlement = _context.DisbursementsAndClaimsMasters.Where(d => d.EmployeeId == id && d.RequestTypeId == (int)ERequestType.CashAdvance && d.IsSettledAmountCredited == false && d.ApprovalStatusId == (int)EApprovalStatus.Approved)
                                                        .Select(s => s.AmountToCredit ?? 0).Sum();

                empAllCurBalStatusDTO.PendingApprovalER = _context.DisbursementsAndClaimsMasters.Where(d => d.EmployeeId == id && d.RequestTypeId == (int)ERequestType.ExpenseReim && d.IsSettledAmountCredited == false && d.ApprovalStatusId == (int)EApprovalStatus.Approved)
                                                       .Select(s => s.AmountToCredit ?? 0).Sum();

                empAllCurBalStatusDTO.PendingApprovalCA = _context.DisbursementsAndClaimsMasters.Where(d => d.EmployeeId == id && d.IsSettledAmountCredited == false && d.ApprovalStatusId == (int)EApprovalStatus.Pending)
                                                        .Select(s => s.ClaimAmount).Sum();

                empAllCurBalStatusDTO.WalletBalLastUpdated = EmpCurrentCashAdvanceBalance.UpdatedOn;

            });
            return empAllCurBalStatusDTO;
        }

        // PUT: api/EmpCurrentCashAdvanceBalances/5
        [HttpPut("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutEmpCurrentCashAdvanceBalance(int id, EmpCurrentCashAdvanceBalanceDTO EmpCurrentCashAdvanceBalanceDto)
        {
            if (id != EmpCurrentCashAdvanceBalanceDto.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }

            var EmpCurrentCashAdvanceBalance = await _context.EmpCurrentCashAdvanceBalances.FindAsync(id);

            EmpCurrentCashAdvanceBalance.Id = EmpCurrentCashAdvanceBalanceDto.Id;
            EmpCurrentCashAdvanceBalance.EmployeeId = EmpCurrentCashAdvanceBalanceDto.EmployeeId;
            EmpCurrentCashAdvanceBalance.CurBalance = EmpCurrentCashAdvanceBalanceDto.CurBalance;
            EmpCurrentCashAdvanceBalance.UpdatedOn = EmpCurrentCashAdvanceBalanceDto.UpdatedOn;

            _context.EmpCurrentCashAdvanceBalances.Update(EmpCurrentCashAdvanceBalance);
            //_context.Entry(projectDto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmpCurrentCashAdvanceBalanceExists(id))
                {
                    return Conflict(new RespStatus { Status = "Failure", Message = "Currnet Balance Id invalid!" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new RespStatus { Status = "Success", Message = "Petty Cash Balance Details Updated!" });
        }

        // POST: api/EmpCurrentCashAdvanceBalances
        [HttpPost]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<ActionResult<EmpCurrentCashAdvanceBalance>> PostEmpCurrentCashAdvanceBalance(EmpCurrentCashAdvanceBalanceDTO EmpCurrentCashAdvanceBalanceDto)
        {
            EmpCurrentCashAdvanceBalance EmpCurrentCashAdvanceBalance = new()
            {
                Id = EmpCurrentCashAdvanceBalanceDto.Id,
                EmployeeId = EmpCurrentCashAdvanceBalanceDto.EmployeeId,
                CurBalance = EmpCurrentCashAdvanceBalanceDto.CurBalance,
                CashOnHand = 0,
                UpdatedOn = EmpCurrentCashAdvanceBalanceDto.UpdatedOn
            };

            _context.EmpCurrentCashAdvanceBalances.Add(EmpCurrentCashAdvanceBalance);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmpCurrentCashAdvanceBalance", new { id = EmpCurrentCashAdvanceBalance.Id }, EmpCurrentCashAdvanceBalance);
        }

        // GET: api/EmpCurrentCashAdvanceBalances/GetEmpCashBalanceVsAdvanced
        [HttpGet("{id}")]
        [ActionName("GetEmpCashBalanceVsAdvanced")]
        public async Task<ActionResult<CashbalVsAdvancedVM>> GetEmpCashBalanceVsAdvanced(int id)
        {

            CashbalVsAdvancedVM cashbalVsAdvancedVM = new();
            if (id == 0) // atominos admin doesnt have a wallet balance
            {
                cashbalVsAdvancedVM.CurCashBal = 0;
                cashbalVsAdvancedVM.MaxCashAllowed = 0;
                return Ok(cashbalVsAdvancedVM);
            }

            //int test = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == id).FirstOrDefault().Id;
            var empCurPettyBal = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == id).FirstOrDefault();
            //Check if employee cash balance is availabel in the EmpCurrentCashAdvanceBalance table, if NOT then ADD
            if (empCurPettyBal == null)
            {
                var emp = _context.Employees.Find(id);

                if (emp != null)
                {

                    Double? empPettyCashAmountEligible = empCurPettyBal.MaxCashAdvanceLimit;

                    _context.EmpCurrentCashAdvanceBalances.Add(new EmpCurrentCashAdvanceBalance()
                    {
                        EmployeeId = id,
                        CurBalance = empPettyCashAmountEligible,
                        CashOnHand = 0,
                        UpdatedOn = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();
                }
            }
           
            if (empCurPettyBal == null)
            {
                cashbalVsAdvancedVM.CurCashBal = 0;
                cashbalVsAdvancedVM.MaxCashAllowed = 0;
            }
            else
            {
                cashbalVsAdvancedVM.CurCashBal = empCurPettyBal.CurBalance;
            
            
            }
            return Ok(cashbalVsAdvancedVM);
        }



        // DELETE: api/EmpCurrentCashAdvanceBalances/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteEmpCurrentCashAdvanceBalance(int id)
        {
            var EmpCurrentCashAdvanceBalance = await _context.EmpCurrentCashAdvanceBalances.FindAsync(id);
            if (EmpCurrentCashAdvanceBalance == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "EmpCurrentCashAdvanceBalances Id invalid!" });
            }

            _context.EmpCurrentCashAdvanceBalances.Remove(EmpCurrentCashAdvanceBalance);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Cash Balance Deleted!" });
        }

        private bool EmpCurrentCashAdvanceBalanceExists(int id)
        {
            return _context.EmpCurrentCashAdvanceBalances.Any(e => e.Id == id);
        }



        //
    }
}
