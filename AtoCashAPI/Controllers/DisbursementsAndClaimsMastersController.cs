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
using System.IO;
using Microsoft.Extensions.Logging;
using EmailService;
using System.Net.Mail;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Text;

namespace AtoCashAPI.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize(Roles = "Admin, Finmgr, AccPayable, User")]
    public class DisbursementsAndClaimsMastersController : ControllerBase
    {
        private readonly AtoCashDbContext _context;
        private readonly ILogger<DisbursementsAndClaimsMastersController> _logger;
        private readonly IEmailSender _emailSender;

        public DisbursementsAndClaimsMastersController(AtoCashDbContext context, ILogger<DisbursementsAndClaimsMastersController> logger, IEmailSender emailSender)
        {
            _context = context;
            _logger = logger;
            _emailSender = emailSender;
        }

        // GET: api/DisbursementsAndClaimsMasters/
        [HttpGet]
        [ActionName("GetDisbursementsAndClaimsMasters")]
        public async Task<ActionResult<IEnumerable<DisbursementsAndClaimsMasterDTO>>> GetDisbursementsAndClaimsMasters()
        {
            List<DisbursementsAndClaimsMasterDTO> ListDisbursementsAndClaimsMasterDTO = new();

            var disbursementsAndClaimsMasters = await _context.DisbursementsAndClaimsMasters.Where(d => d.RequestTypeId == (int)ERequestType.ExpenseReim && d.IsSettledAmountCredited == false).ToListAsync();

            foreach (DisbursementsAndClaimsMaster disbursementsAndClaimsMaster in disbursementsAndClaimsMasters)
            {
                DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO = new();

                disbursementsAndClaimsMasterDTO.Id = disbursementsAndClaimsMaster.Id;
                disbursementsAndClaimsMasterDTO.EmployeeId = disbursementsAndClaimsMaster.EmployeeId;
                disbursementsAndClaimsMasterDTO.EmployeeName = _context.Employees.Find(disbursementsAndClaimsMaster.EmployeeId).GetFullName();
                disbursementsAndClaimsMasterDTO.BlendedRequestId = disbursementsAndClaimsMaster.BlendedRequestId;

                disbursementsAndClaimsMasterDTO.BusinessTypeId = disbursementsAndClaimsMaster.BusinessTypeId;
                disbursementsAndClaimsMasterDTO.BusinessType = _context.BusinessTypes.Find(disbursementsAndClaimsMaster.BusinessTypeId).BusinessTypeName;
                disbursementsAndClaimsMasterDTO.BusinessUnitId = disbursementsAndClaimsMaster.BusinessUnitId;
                disbursementsAndClaimsMasterDTO.BusinessUnit = _context.BusinessUnits.Find(disbursementsAndClaimsMaster.BusinessUnitId).GetBusinessUnitName();

                disbursementsAndClaimsMasterDTO.ProjectId = disbursementsAndClaimsMaster.ProjectId;
                disbursementsAndClaimsMasterDTO.ProjectName = disbursementsAndClaimsMaster.ProjectId != null ? _context.Projects.Find(disbursementsAndClaimsMaster.ProjectId).ProjectName : string.Empty;
                disbursementsAndClaimsMasterDTO.SubProjectId = disbursementsAndClaimsMaster.SubProjectId;
                disbursementsAndClaimsMasterDTO.SubProjectName = disbursementsAndClaimsMaster.SubProjectId != null ? _context.SubProjects.Find(disbursementsAndClaimsMaster.SubProjectId).SubProjectName : string.Empty;
                disbursementsAndClaimsMasterDTO.WorkTaskId = disbursementsAndClaimsMaster.WorkTaskId;
                disbursementsAndClaimsMasterDTO.WorkTaskName = disbursementsAndClaimsMaster.WorkTaskId != null ? _context.WorkTasks.Find(disbursementsAndClaimsMaster.WorkTaskId).TaskName : string.Empty;
                disbursementsAndClaimsMasterDTO.CurrencyTypeId = disbursementsAndClaimsMaster.CurrencyTypeId;
                disbursementsAndClaimsMasterDTO.CurrencyType = _context.CurrencyTypes.Find(disbursementsAndClaimsMaster.CurrencyTypeId).CurrencyCode;
                disbursementsAndClaimsMasterDTO.RecordDate = disbursementsAndClaimsMaster.RecordDate.ToString(); //ToShortDateString();
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

            return ListDisbursementsAndClaimsMasterDTO;
        }





        // GET: api/DisbursementsAndClaimsMasters/5
        [HttpGet("{id}")]
        [ActionName("GetDisbursementsAndClaimsMaster")]
        public async Task<ActionResult<DisbursementsAndClaimsMasterDTO>> GetDisbursementsAndClaimsMaster(int id)
        {


            var disbursementsAndClaimsMaster = await _context.DisbursementsAndClaimsMasters.FindAsync(id);

            if (disbursementsAndClaimsMaster == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Disburese Id invalid!" });
            }

            DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO = new();

            disbursementsAndClaimsMasterDTO.Id = disbursementsAndClaimsMaster.Id;
            disbursementsAndClaimsMasterDTO.EmployeeId = disbursementsAndClaimsMaster.EmployeeId;
            disbursementsAndClaimsMasterDTO.EmployeeName = _context.Employees.Find(disbursementsAndClaimsMaster.EmployeeId).GetFullName();
            disbursementsAndClaimsMasterDTO.BlendedRequestId = disbursementsAndClaimsMaster.BlendedRequestId;

            disbursementsAndClaimsMasterDTO.BusinessTypeId = disbursementsAndClaimsMaster.BusinessTypeId;
            disbursementsAndClaimsMasterDTO.BusinessType = disbursementsAndClaimsMaster.BusinessTypeId != null ?_context.BusinessTypes.Find(disbursementsAndClaimsMaster.BusinessTypeId).BusinessTypeName : null;
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
            disbursementsAndClaimsMasterDTO.RecordDate = disbursementsAndClaimsMaster.RecordDate.ToString(); //ToShortDateString();
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




            return Ok(disbursementsAndClaimsMasterDTO);
        }

        [HttpPut("{id}")]
        [ActionName("SettleAccountsSAP")]
        [Authorize(Roles = "AccPayable")] // Only AccountPayables clerk can upate DisbursementsAndClaimsMaster
        public async Task<IActionResult> SettleAccountsSAP(int[] accPayableSettleClaimIds)
        {
            string SAPApiPostUrl = "https://localhost:44324/api/Reservation";

            if (accPayableSettleClaimIds != null)
            {
                // List<PostSAPAPIData> postSAPAPIData = new List<PostSAPAPIData>();

                foreach (var SettleClaimId in accPayableSettleClaimIds)
                {
                    PostSAPAPIData postSAPApiData = new PostSAPAPIData();

                    var disbclaim = await _context.DisbursementsAndClaimsMasters.FindAsync(SettleClaimId);
                    var employee = await _context.Employees.FindAsync(disbclaim.EmployeeId);
                    //var expReimb = disbclaim.RequestTypeId == 1 ? true : false;

                    postSAPApiData.ClaimId = SettleClaimId;
                    postSAPApiData.EmployeeName = employee.GetFullName();
                    postSAPApiData.EmployeeCode = employee.EmpCode;

                    postSAPApiData.BusinessType = _context.BusinessTypes.Find(disbclaim.BusinessTypeId).BusinessTypeName;
                    postSAPApiData.BusinessUnit = _context.BusinessUnits.Find(disbclaim.BusinessUnitId).GetBusinessUnitName();

                    postSAPApiData.Project = disbclaim.ProjectId != null ? _context.Projects.Find(disbclaim.ProjectId).ProjectName : null;
                    postSAPApiData.RequestDate = disbclaim.RecordDate;
                    postSAPApiData.ClaimAmount = disbclaim.ClaimAmount;
                    postSAPApiData.AmountToWallet = disbclaim.AmountToWallet;
                    postSAPApiData.AmountToBank = disbclaim.AmountToCredit;
                    postSAPApiData.Status = _context.ApprovalStatusTypes.Find(disbclaim.ApprovalStatusId).Status;
                    postSAPApiData.Action = "To be Settled, and Amount Credited to Bank";
                    postSAPApiData.IsCashAdvanceRequest = disbclaim.RequestTypeId == 1 ? true : false; //1- Cash Advance req, 2- Exp Reimburse


                    List<PostSubClaimItems> ListPostSubClaimItem = new List<PostSubClaimItems>();
                    if (postSAPApiData.IsCashAdvanceRequest == false)
                    {
                        var expReimbReq = _context.ExpenseReimburseRequests.Find(disbclaim.BlendedRequestId);

                        List<ExpenseSubClaim> expenseSubClaims = _context.ExpenseSubClaims.Where(e => e.ExpenseReimburseRequestId == disbclaim.BlendedRequestId).ToList();

                        foreach (var expenseSubClaim in expenseSubClaims)
                        {
                            PostSubClaimItems postSubClaimItem = new PostSubClaimItems();

                            var exptype = await _context.ExpenseTypes.FindAsync(expenseSubClaim.ExpenseTypeId);
                            postSubClaimItem.SubClaimId = expenseSubClaim.Id;
                            postSubClaimItem.CostCentre = _context.CostCenters.Find(expenseSubClaim.CostCenterId).CostCenterCode;
                            postSubClaimItem.GeneralLedger = _context.GeneralLedger.Find(exptype.GeneralLedgerId).GeneralLedgerAccountNo;
                            postSubClaimItem.SubClaimAmount = expenseSubClaim.ExpenseReimbClaimAmount;
                            postSubClaimItem.InvoiceNo = expenseSubClaim.InvoiceNo;
                            postSubClaimItem.InvoiceDate = expenseSubClaim.InvoiceDate;
                            postSubClaimItem.Vendor = _context.Vendors.Find(expenseSubClaim.VendorId).VendorName;
                            postSubClaimItem.ExpenseType = exptype.ExpenseTypeName;

                            ListPostSubClaimItem.Add(postSubClaimItem);
                        }

                    }

                    postSAPApiData.SubClaimItems = ListPostSubClaimItem;


                }

                PostSAPAPIData postSAPAPIdata = new PostSAPAPIData();

                var jsonString = JsonConvert.SerializeObject(postSAPAPIdata);
                var SAPAPIDataContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
                {

                    using (var httpClient = new HttpClient())
                    {
                        using (var response = await httpClient.PostAsync(SAPApiPostUrl, SAPAPIDataContent))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            var ResponseRetunSAPApiData = JsonConvert.DeserializeObject<ResponseSAPApiData>(apiResponse);


                            //if (ResponseRetunSAPApiData.StatusCode == System.Net.HttpStatusCode.OK)
                            //{
                            //    TempData["Profile"] = JsonConvert.SerializeObject(user);
                            //    return RedirectToAction("Index");
                            //}
                            //else if (Response.StatusCode == System.Net.HttpStatusCode.Conflict)
                            //{
                            //    ModelState.Clear();
                            //    ModelState.AddModelError("Username", "Username Already Exist");
                            //    return View();
                            //}
                        }
                    }







                    await AtoCashDbContextTransaction.CommitAsync();
                }

                return Ok(new RespStatus { Status = "Failure", Message = "SettleAccountsSAP is null!" });
            }
            else
            {
                _logger.LogInformation("List of SettleAccountsSAP ids are null!");
                return Ok(new RespStatus { Status = "Failure", Message = "SettleAccountsSAP is null!" });
            }
            //  RequestSettleClaims requestSettleClaims = new();






        }


        // PUT: api/DisbursementsAndClaimsMasters/5
        [HttpPut("{id}")]
        [ActionName("PutDisbursementsAndClaimsMaster")]
        [Authorize(Roles = "AccPayable")] // Only AccountPayables clerk can upate DisbursementsAndClaimsMaster
        public async Task<IActionResult> PutDisbursementsAndClaimsMaster(int id, DisbursementsAndClaimsMasterDTO disbursementsAndClaimsMasterDTO)
        {
            if (id != disbursementsAndClaimsMasterDTO.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id state is invalid" });
            }

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {
                var disbursementsAndClaimsMaster = await _context.DisbursementsAndClaimsMasters.FindAsync(id);

                double? RoleMaxLimit = 0;

                if (disbursementsAndClaimsMaster.ProjectId == null)
                {
                    //check the Credit To Wallet and Credit to Bank details to adjust for CashOnhand in EmpCurrentCashAdvanceBalance.CashOnHand
                    var empExtInfo = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == disbursementsAndClaimsMaster.EmployeeId && e.BusinessUnitId == disbursementsAndClaimsMaster.BusinessUnitId).FirstOrDefault();

                    RoleMaxLimit = _context.JobRoles.Find(empExtInfo.JobRoleId).MaxCashAdvanceAllowed;
                }
                else
                {

                    RoleMaxLimit = _context.EmpCurrentCashAdvanceBalances.Where(e=> e.EmployeeId == disbursementsAndClaimsMaster.EmployeeId).FirstOrDefault().MaxCashAdvanceLimit;
                }

                double CreditToWallet = disbursementsAndClaimsMaster.AmountToWallet ?? 0;
                EmpCurrentCashAdvanceBalance empCashAdvanceBal = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == disbursementsAndClaimsMaster.EmployeeId).FirstOrDefault();

                //if CashAdvance Request then update the CashOnHand in EmpCashAdvance Balances table

                //CashAdvance Request
                if (disbursementsAndClaimsMaster.BlendedRequestId != null && disbursementsAndClaimsMaster.RequestTypeId == 1)
                {

                    empCashAdvanceBal.CashOnHand = empCashAdvanceBal.CashOnHand + disbursementsAndClaimsMaster.AmountToCredit ?? 0;
                    //empCashAdvanceBal.CurBalance = empCashAdvanceBal.CurBalance - empCashAdvanceBal.CashOnHand;
                   // empCashAdvanceBal.CurBalance = (empCashAdvanceBal.CurBalance - empCashAdvanceBal.CashOnHand) >= 0 ? (empCashAdvanceBal.CurBalance - empCashAdvanceBal.CashOnHand) : RoleMaxLimit;
                }
                else
                {

                    empCashAdvanceBal.CashOnHand = (empCashAdvanceBal.CashOnHand - CreditToWallet) >= 0 ? empCashAdvanceBal.CashOnHand - CreditToWallet : 0;
                    empCashAdvanceBal.CurBalance = (empCashAdvanceBal.CurBalance + (disbursementsAndClaimsMaster.AmountToWallet ?? 0)) < RoleMaxLimit ? (empCashAdvanceBal.CurBalance + (disbursementsAndClaimsMaster.AmountToWallet ?? 0)) : RoleMaxLimit;

                    //curbalance cannot be more than the RoleMax lime for cash requests
                    if (empCashAdvanceBal.CurBalance > RoleMaxLimit)
                    {
                        empCashAdvanceBal.CurBalance = RoleMaxLimit;
                    }
                }

                _context.Update(empCashAdvanceBal);



                disbursementsAndClaimsMaster.IsSettledAmountCredited = true;
                disbursementsAndClaimsMaster.SettledDate = DateTime.UtcNow;
                disbursementsAndClaimsMaster.SettlementComment = disbursementsAndClaimsMasterDTO.SettlementComment;
                disbursementsAndClaimsMaster.SettlementAccount = disbursementsAndClaimsMasterDTO.SettlementAccount;
                disbursementsAndClaimsMaster.SettlementBankCard = disbursementsAndClaimsMasterDTO.SettlementBankCard;
                disbursementsAndClaimsMaster.AdditionalData = disbursementsAndClaimsMasterDTO.AdditionalData;

                _context.DisbursementsAndClaimsMasters.Update(disbursementsAndClaimsMaster);
                //_context.Entry(disbursementsAndClaimsMasterDTO).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }


                /// Email to requester start
                /// 
                Employee requester = _context.Employees.Find(disbursementsAndClaimsMaster.EmployeeId);

                var requesterMailAddress = requester.Email;

                _logger.LogInformation(requester.GetFullName() + "Settlement Email Start");

                string[] paths = { Directory.GetCurrentDirectory(), "ClaimApprovedandSettled.html" };
                string FilePath = Path.Combine(paths);
                _logger.LogInformation("Email template path " + FilePath);
                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();


                var requestId = disbursementsAndClaimsMaster.BlendedRequestId;

                string subject = "Claim request is processed and settled for Request No: " + requestId;

                var builder = new MimeKit.BodyBuilder();

                MailText = MailText.Replace("{Requester}", requester.GetFullName());
                MailText = MailText.Replace("{Currency}", _context.CurrencyTypes.Find(requester.CurrencyTypeId).CurrencyCode);
                MailText = MailText.Replace("{RequestedAmount}", disbursementsAndClaimsMaster.AmountToWallet.ToString() + " + " + disbursementsAndClaimsMaster.AmountToCredit.ToString());
                MailText = MailText.Replace("{RequestNumber}", requestId.ToString());
                builder.HtmlBody = MailText;

                var messagemail = new Message(new string[] { requesterMailAddress }, subject, builder.HtmlBody);

                await _emailSender.SendEmailAsync(messagemail);
                _logger.LogInformation(requester.GetFullName() + "Settlement Email Sent");


                await AtoCashDbContextTransaction.CommitAsync();
            }
            return Ok(new RespStatus { Status = "Success", Message = "Accounts Payable Entry Updated!" });
        }



    }
}
