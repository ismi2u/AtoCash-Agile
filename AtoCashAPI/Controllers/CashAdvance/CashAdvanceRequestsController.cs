using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtoCashAPI.Data;
using AtoCashAPI.Models;
using EmailService;
using AtoCashAPI.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.Extensions.Logging;


namespace AtoCashAPI.Controllers.CashAdvance
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User")]

    public class CashAdvanceRequestsController : ControllerBase
    {
        private readonly AtoCashDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CashAdvanceRequestsController> _logger;
        private readonly IConfiguration _config;

        public CashAdvanceRequestsController(AtoCashDbContext context, IEmailSender emailSender,
            ILogger<CashAdvanceRequestsController> logger,
            IConfiguration config)
        {
            this._context = context;
            this._emailSender = emailSender;
            _logger = logger;
            _config = config;
        }


        // GET: api/CashAdvanceRequests
        [HttpGet]
        [ActionName("GetCashAdvanceRequests")]
        public async Task<ActionResult<IEnumerable<CashAdvanceRequestDTO>>> GetCashAdvanceRequests()
        {
            List<CashAdvanceRequestDTO> ListCashAdvanceRequestDTO = new();  

            //var CashAdvanceStatusTracker = await _context.CashAdvanceStatusTrackers.FindAsync(1);

            var CashAdvanceRequests = await _context.CashAdvanceRequests.ToListAsync();
            if (CashAdvanceRequests == null)
            {
                _logger.LogError("GetCashAdvanceRequests is null");
            }

            foreach (CashAdvanceRequest CashAdvanceRequest in CashAdvanceRequests)
            {
                CashAdvanceRequestDTO CashAdvanceRequestDTO = new();

                CashAdvanceRequestDTO.Id = CashAdvanceRequest.Id;
                CashAdvanceRequestDTO.EmployeeName = _context.Employees.Find(CashAdvanceRequest.EmployeeId).GetFullName();
                CashAdvanceRequestDTO.CurrencyTypeId = CashAdvanceRequest.CurrencyTypeId;
                CashAdvanceRequestDTO.CurrencyType = CashAdvanceRequest.CurrencyType != null ? _context.CurrencyTypes.Find(CashAdvanceRequest.CurrencyType).CurrencyName : null;
                CashAdvanceRequestDTO.CashAdvanceAmount = CashAdvanceRequest.CashAdvanceAmount;
                CashAdvanceRequestDTO.CashAdvanceRequestDesc = CashAdvanceRequest.CashAdvanceRequestDesc;
                CashAdvanceRequestDTO.RequestDate = CashAdvanceRequest.RequestDate;

                CashAdvanceRequestDTO.BusinessTypeId = CashAdvanceRequest.BusinessTypeId;
                CashAdvanceRequestDTO.BusinessType = CashAdvanceRequest.BusinessTypeId != null ? _context.BusinessTypes.Find(CashAdvanceRequest.BusinessTypeId).BusinessTypeName : null;
                CashAdvanceRequestDTO.BusinessUnitId = CashAdvanceRequest.BusinessUnitId;
                CashAdvanceRequestDTO.BusinessUnit = CashAdvanceRequest.BusinessUnitId != null ? _context.BusinessUnits.Find(CashAdvanceRequest.BusinessUnitId).GetBusinessUnitName() : null;

                if (CashAdvanceRequest.BusinessUnitId != null)
                {
                    var locationId = _context.BusinessUnits.Find(CashAdvanceRequest.BusinessUnitId).LocationId;
                    CashAdvanceRequestDTO.Location = _context.Locations.Find(locationId).LocationName;
                }

                CashAdvanceRequestDTO.CostCenterId = CashAdvanceRequest.CostCenterId;
                CashAdvanceRequestDTO.CostCentre = CashAdvanceRequest.CostCenterId != null ? _context.CostCenters.Find(CashAdvanceRequest.CostCenterId).GetCostCentre() : null;

                CashAdvanceRequestDTO.ProjectId = CashAdvanceRequest.ProjectId;
                CashAdvanceRequestDTO.Project = CashAdvanceRequest.ProjectId != null ? _context.Projects.Find(CashAdvanceRequest.ProjectId).ProjectName : null;
                CashAdvanceRequestDTO.SubProjectId = CashAdvanceRequest.SubProjectId;
                CashAdvanceRequestDTO.SubProject = CashAdvanceRequest.SubProjectId != null ? _context.SubProjects.Find(CashAdvanceRequest.SubProjectId).SubProjectName : null;
                CashAdvanceRequestDTO.WorkTaskId = CashAdvanceRequest.WorkTaskId;
                CashAdvanceRequestDTO.WorkTask = CashAdvanceRequest.WorkTaskId != null ? _context.WorkTasks.Find(CashAdvanceRequest.WorkTaskId).TaskName : null;
                CashAdvanceRequestDTO.ApprovalStatusType = CashAdvanceRequest.ApprovalStatusTypeId != 0 ? _context.ApprovalStatusTypes.Find(CashAdvanceRequest.ApprovalStatusTypeId).Status : null;
                CashAdvanceRequestDTO.ApprovalStatusTypeId = CashAdvanceRequest.ApprovalStatusTypeId;
                CashAdvanceRequestDTO.ApproverActionDate = CashAdvanceRequest.ApproverActionDate;
                ListCashAdvanceRequestDTO.Add(CashAdvanceRequestDTO);
            }

            return ListCashAdvanceRequestDTO.OrderByDescending(o => o.RequestDate).ToList();
        }



        // GET: api/CashAdvanceRequests/5
        [HttpGet("{id}")]
        [ActionName("GetCashAdvanceRequest")]
        public async Task<ActionResult<CashAdvanceRequestDTO>> GetCashAdvanceRequest(int id)
        {
            var CashAdvanceRequest = await _context.CashAdvanceRequests.FindAsync(id);

            var disbAndClaim = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == id && d.RequestTypeId == (int)ERequestType.CashAdvance).FirstOrDefault();

            if (CashAdvanceRequest == null)
            {
                _logger.LogError("GetCashAdvanceRequests: Cash Advance RequestId is Invalid:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Cash Advance RequestId is Invalid!" });
            }
            CashAdvanceRequestDTO CashAdvanceRequestDTO = new();

            CashAdvanceRequestDTO.Id = CashAdvanceRequest.Id;
            CashAdvanceRequestDTO.EmployeeName = _context.Employees.Find(CashAdvanceRequest.EmployeeId).GetFullName();
            CashAdvanceRequestDTO.CurrencyTypeId = CashAdvanceRequest.CurrencyTypeId;
            CashAdvanceRequestDTO.CurrencyType = CashAdvanceRequest.CurrencyType != null ? _context.CurrencyTypes.Find(CashAdvanceRequest.CurrencyType).CurrencyName : null;
            CashAdvanceRequestDTO.CashAdvanceAmount = CashAdvanceRequest.CashAdvanceAmount;
            CashAdvanceRequestDTO.CashAdvanceRequestDesc = CashAdvanceRequest.CashAdvanceRequestDesc;
            CashAdvanceRequestDTO.RequestDate = CashAdvanceRequest.RequestDate;

            CashAdvanceRequestDTO.BusinessTypeId = CashAdvanceRequest.BusinessTypeId;
            CashAdvanceRequestDTO.BusinessType = CashAdvanceRequest.BusinessTypeId != null ? _context.BusinessTypes.Find(CashAdvanceRequest.BusinessTypeId).BusinessTypeName : null;
            CashAdvanceRequestDTO.BusinessUnitId = CashAdvanceRequest.BusinessUnitId;
            CashAdvanceRequestDTO.BusinessUnit = CashAdvanceRequest.BusinessUnitId != null ? _context.BusinessUnits.Find(CashAdvanceRequest.BusinessUnitId).GetBusinessUnitName() : null;

            if (CashAdvanceRequest.BusinessUnitId != null)
            {
                var locationId = _context.BusinessUnits.Find(CashAdvanceRequest.BusinessUnitId).LocationId;
                CashAdvanceRequestDTO.Location = _context.Locations.Find(locationId).LocationName;
            }

            CashAdvanceRequestDTO.CostCenterId = CashAdvanceRequest.CostCenterId;
            CashAdvanceRequestDTO.CostCentre = CashAdvanceRequest.CostCenterId != null ? _context.CostCenters.Find(CashAdvanceRequest.CostCenterId).GetCostCentre() : null;


            CashAdvanceRequestDTO.ProjectId = CashAdvanceRequest.ProjectId;
            CashAdvanceRequestDTO.Project = CashAdvanceRequest.ProjectId != null ? _context.Projects.Find(CashAdvanceRequest.ProjectId).ProjectName : null;
            CashAdvanceRequestDTO.SubProjectId = CashAdvanceRequest.SubProjectId;
            CashAdvanceRequestDTO.SubProject = CashAdvanceRequest.SubProjectId != null ? _context.SubProjects.Find(CashAdvanceRequest.SubProjectId).SubProjectName : null;
            CashAdvanceRequestDTO.WorkTaskId = CashAdvanceRequest.WorkTaskId;
            CashAdvanceRequestDTO.WorkTask = CashAdvanceRequest.WorkTaskId != null ? _context.WorkTasks.Find(CashAdvanceRequest.WorkTaskId).TaskName : null;
            CashAdvanceRequestDTO.ApprovalStatusTypeId = CashAdvanceRequest.ApprovalStatusTypeId;
            CashAdvanceRequestDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(CashAdvanceRequest.ApprovalStatusTypeId).Status;
            CashAdvanceRequestDTO.ApproverActionDate = CashAdvanceRequest.ApproverActionDate;
            CashAdvanceRequestDTO.CreditToBank = CashAdvanceRequest.ApprovalStatusTypeId == (int)EApprovalStatus.Approved ? disbAndClaim.AmountToCredit : 0;
            CashAdvanceRequestDTO.IsSettled = (disbAndClaim.IsSettledAmountCredited ?? false);

            CashAdvanceRequestDTO.Comments = CashAdvanceRequest.Comments;

            return CashAdvanceRequestDTO;
        }


        [HttpGet("{id}")]
        [ActionName("GetCashAdvanceRequestRaisedForEmployee")]
        public async Task<ActionResult<IEnumerable<CashAdvanceRequestDTO>>> GetCashAdvanceRequestRaisedForEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            var ListEmpExtInfos = await _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == id).ToListAsync();


            if (employee == null)
            {
                _logger.LogError("Cash Advance : Employee Id is not valid:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id is Invalid!" });
            }

            var ListEmpCashAdvanceRequests = await _context.CashAdvanceRequests.Where(p => p.EmployeeId == id).ToListAsync();

            if (ListEmpCashAdvanceRequests == null)
            {
                _logger.LogError("Cash Advance: Cash Advance Request is null");
                return Ok(new RespStatus { Status = "Success", Message = "No Cash Advance Requests raised!" });
            }

            List<CashAdvanceRequestDTO> CashAdvanceRequestDTOs = new();


            foreach (var CashAdvanceRequest in ListEmpCashAdvanceRequests)
            {
                CashAdvanceRequestDTO CashAdvanceRequestDTO = new();

                CashAdvanceRequestDTO.Id = CashAdvanceRequest.Id;
                CashAdvanceRequestDTO.EmployeeName = _context.Employees.Find(CashAdvanceRequest.EmployeeId).GetFullName();
                CashAdvanceRequestDTO.CurrencyTypeId = CashAdvanceRequest.CurrencyTypeId;
                CashAdvanceRequestDTO.CurrencyType = CashAdvanceRequest.CurrencyType != null ? _context.CurrencyTypes.Find(CashAdvanceRequest.CurrencyType).CurrencyName : null;
                CashAdvanceRequestDTO.CashAdvanceAmount = CashAdvanceRequest.CashAdvanceAmount;
                CashAdvanceRequestDTO.CashAdvanceRequestDesc = CashAdvanceRequest.CashAdvanceRequestDesc;
                CashAdvanceRequestDTO.RequestDate = CashAdvanceRequest.RequestDate;

                CashAdvanceRequestDTO.BusinessTypeId = CashAdvanceRequest.BusinessTypeId;
                CashAdvanceRequestDTO.BusinessType = CashAdvanceRequest.BusinessTypeId != null ? _context.BusinessTypes.Find(CashAdvanceRequest.BusinessTypeId).BusinessTypeName : null;
                CashAdvanceRequestDTO.BusinessUnitId = CashAdvanceRequest.BusinessUnitId;
                CashAdvanceRequestDTO.BusinessUnit = CashAdvanceRequest.BusinessUnitId != null ? _context.BusinessUnits.Find(CashAdvanceRequest.BusinessUnitId).GetBusinessUnitName() : null;

                if (CashAdvanceRequest.BusinessUnitId != null)
                {
                    var locationId = _context.BusinessUnits.Find(CashAdvanceRequest.BusinessUnitId).LocationId;
                    CashAdvanceRequestDTO.Location = _context.Locations.Find(locationId).LocationName;
                }

                CashAdvanceRequestDTO.CostCenterId = CashAdvanceRequest.CostCenterId;
                CashAdvanceRequestDTO.CostCentre = CashAdvanceRequest.CostCenterId != null ? _context.CostCenters.Find(CashAdvanceRequest.CostCenterId).GetCostCentre() : null;


                CashAdvanceRequestDTO.ProjectId = CashAdvanceRequest.ProjectId;
                CashAdvanceRequestDTO.Project = CashAdvanceRequest.ProjectId != null ? _context.Projects.Find(CashAdvanceRequest.ProjectId).ProjectName : null;
                CashAdvanceRequestDTO.SubProjectId = CashAdvanceRequest.SubProjectId;
                CashAdvanceRequestDTO.SubProject = CashAdvanceRequest.SubProjectId != null ? _context.SubProjects.Find(CashAdvanceRequest.SubProjectId).SubProjectName : null;
                CashAdvanceRequestDTO.WorkTaskId = CashAdvanceRequest.WorkTaskId;
                CashAdvanceRequestDTO.WorkTask = CashAdvanceRequest.WorkTaskId != null ? _context.WorkTasks.Find(CashAdvanceRequest.WorkTaskId).TaskName : null;
                CashAdvanceRequestDTO.ApprovalStatusTypeId = CashAdvanceRequest.ApprovalStatusTypeId;
                CashAdvanceRequestDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(CashAdvanceRequest.ApprovalStatusTypeId).Status;
                CashAdvanceRequestDTO.ApproverActionDate = CashAdvanceRequest.ApproverActionDate;

                // set the bookean flat to TRUE if No approver has yet approved the Request else FALSE
                bool ifAnyOfStatusRecordsApproved = _context.CashAdvanceStatusTrackers.Where(t =>
                                                           (t.ApprovalStatusTypeId == (int)EApprovalStatus.Rejected ||
                                                          t.ApprovalStatusTypeId == (int)EApprovalStatus.Approved) &&
                                                          t.CashAdvanceRequestId == CashAdvanceRequest.Id).Any();

                CashAdvanceRequestDTO.ShowEditDelete = ifAnyOfStatusRecordsApproved ? false : true;
                ///
                CashAdvanceRequestDTOs.Add(CashAdvanceRequestDTO);
            }

            return Ok(CashAdvanceRequestDTOs.OrderByDescending(o => o.RequestDate).ToList());
        }



        [HttpGet("{id}")]
        [ActionName("CountAllCashAdvanceRequestRaisedByEmployee")]
        public async Task<ActionResult> CountAllCashAdvanceRequestRaisedByEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return Ok(0);
            }

            var CashAdvanceRequests = await _context.CashAdvanceRequests.Where(p => p.EmployeeId == id).ToListAsync();

            if (CashAdvanceRequests == null)
            {
                return Ok(0);
            }

            int TotalCount = _context.CashAdvanceRequests.Where(c => c.EmployeeId == id).Count();
            int PendingCount = _context.CashAdvanceRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).Count();
            int RejectedCount = _context.CashAdvanceRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Rejected).Count();
            int ApprovedCount = _context.CashAdvanceRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Approved).Count();

            return Ok(new { TotalCount, PendingCount, RejectedCount, ApprovedCount });
        }




        [HttpGet]
        [ActionName("GetCashAdvanceReqInPendingForAll")]
        public async Task<ActionResult<int>> GetCashAdvanceReqInPendingForAll()
        {
            //debug
            var CashAdvanceRequests = await _context.CashAdvanceRequests.Include("CashAdvanceStatusTrackers").ToListAsync();


            //var CashAdvanceRequests = await _context.CashAdvanceStatusTrackers.Where(c => c.ApprovalStatusTypeId == ApprovalStatus.Pending).select( );

            if (CashAdvanceRequests == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "CashAdvanceRequests is Empty!" });
            }

            return Ok(CashAdvanceRequests.Count);
        }



        // PUT: api/CashAdvanceRequests/5
        [HttpPut("{id}")]
        [ActionName("PutCashAdvanceRequest")]
        public async Task<IActionResult> PutCashAdvanceRequest(int id, CashAdvanceRequestDTO CashAdvanceRequestDto)
        {
            if (id != CashAdvanceRequestDto.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }

            var CashAdvanceRequest = await _context.CashAdvanceRequests.FindAsync(id);
            CashAdvanceRequestDto.EmployeeId = CashAdvanceRequest.EmployeeId;
            int costcentreId = 0;

           

            Double? empCurAvailBal = GetEmpCurrentAvailableCashAdvanceBalance(CashAdvanceRequestDto);

            if (!(CashAdvanceRequestDto.CashAdvanceAmount <= empCurAvailBal && CashAdvanceRequestDto.CashAdvanceAmount > 0))
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Invalid Cash Request Amount Or Limit Exceeded" });
            }




            int ApprovedCount = _context.CashAdvanceStatusTrackers.Where(e => e.CashAdvanceRequestId == CashAdvanceRequest.Id && e.ApprovalStatusTypeId == (int)EApprovalStatus.Approved).Count();
            if (ApprovedCount != 0)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "CashAdvance Requests cant be Edited after Approval!" });
            }


            //if CashAdvance request is modified then trigger changes to other tables
            if (CashAdvanceRequest.CashAdvanceAmount != CashAdvanceRequestDto.CashAdvanceAmount)
            {

                //update the EmpCashAdvanceBalance to credit back the deducted amount
                EmpCurrentCashAdvanceBalance empCashAdvanceBal = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == CashAdvanceRequest.EmployeeId).FirstOrDefault();
                double? oldBal = empCashAdvanceBal.CurBalance;
                double? prevAmt = CashAdvanceRequest.CashAdvanceAmount;
                double? NewAmt = CashAdvanceRequestDto.CashAdvanceAmount;

                CashAdvanceRequest.CashAdvanceAmount = CashAdvanceRequestDto.CashAdvanceAmount;
                CashAdvanceRequest.CashAdvanceRequestDesc = CashAdvanceRequestDto.CashAdvanceRequestDesc;
                if (CashAdvanceRequestDto.ProjectId != null)
                {
                    costcentreId = _context.Projects.Find(CashAdvanceRequestDto.ProjectId).CostCenterId;
                }
                else
                {
                    costcentreId = CashAdvanceRequest.CostCenterId;
                }
              

                //check employee allowed limit to Cash Advance, if limit exceeded return with an conflict message.
                var empExtInfo = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == CashAdvanceRequest.EmployeeId && e.BusinessUnitId == CashAdvanceRequest.BusinessUnitId).FirstOrDefault();
                int reqJobRoleId = empExtInfo.JobRoleId ?? 0;
                double maxAllowed = _context.JobRoles.Find(reqJobRoleId).MaxCashAdvanceAllowed ?? 0;
                if (maxAllowed >= oldBal + prevAmt - NewAmt && oldBal + prevAmt - NewAmt > 0)
                {
                    empCashAdvanceBal.CurBalance = oldBal + prevAmt - NewAmt;
                    empCashAdvanceBal.UpdatedOn = DateTime.UtcNow;
                    _context.EmpCurrentCashAdvanceBalances.Update(empCashAdvanceBal);
                }
                else
                {
                    return Conflict(new RespStatus() { Status = "Failure", Message = "Invalid Cash Request Amount Or Limit Exceeded" });
                }



            }
            ////
            ///

            CashAdvanceRequest.CashAdvanceAmount = CashAdvanceRequestDto.CashAdvanceAmount;
            CashAdvanceRequest.CashAdvanceRequestDesc = CashAdvanceRequestDto.CashAdvanceRequestDesc;
            CashAdvanceRequest.RequestDate = DateTime.UtcNow;

            _context.CashAdvanceRequests.Update(CashAdvanceRequest);




            //Step -2 change the claim approval status tracker records
            var claims = await _context.CashAdvanceStatusTrackers.Where(c => c.CashAdvanceRequestId == CashAdvanceRequestDto.Id).ToListAsync();
            bool IsFirstEmail = true;
           // int? newBusinesTypeId = CashAdvanceRequest.BusinessTypeId;
           // int? newBusinesUnitId = CashAdvanceRequest.BusinessUnitId;
            int? newProjId = CashAdvanceRequestDto.ProjectId;
            int? newSubProjId = CashAdvanceRequestDto.SubProjectId;
            int? newWorkTaskId = CashAdvanceRequestDto.WorkTaskId;


            foreach (CashAdvanceStatusTracker claim in claims)
            {
              //  claim.BusinessTypeId = newBusinesTypeId;
              //  claim.BusinessUnitId = newBusinesUnitId;
                claim.ProjectId = newProjId;
                claim.SubProjectId = newSubProjId;
                claim.WorkTaskId = newWorkTaskId;
                claim.RequestDate = CashAdvanceRequest.RequestDate;
                claim.ApproverActionDate = null;
                //claim.ApprovalStatusTypeId = claim.ApprovalLevelId == 1 ? (int)EApprovalStatus.Pending : (int)EApprovalStatus.Initiating;
                claim.Comments = "Modified Request";

                _context.CashAdvanceStatusTrackers.Update(claim);

                if (IsFirstEmail)
                {
                    var empExtendedInfo = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == claim.JobRoleId && e.ApprovalGroupId == claim.ApprovalGroupId).FirstOrDefault();
                    var approver = await _context.Employees.FindAsync(claim.EmployeeId);
                    var approverMailAddress = approver != null ? approver.Email : "";
                    string subject = "(Modified) CashAdvance Request Approval " + CashAdvanceRequestDto.Id.ToString();
                    Employee? emp = await _context.Employees.FindAsync(CashAdvanceRequestDto.EmployeeId);
                    var CashAdvancereq = _context.CashAdvanceRequests.Find(CashAdvanceRequestDto.Id);

                    _logger.LogInformation(approver.GetFullName() + " Email Start");

                    string[] paths = { Directory.GetCurrentDirectory(), "CashAdvanceApprNotificationEmail.html" };
                    string FilePath = Path.Combine(paths);
                    _logger.LogInformation("Email template path " + FilePath);
                    StreamReader str = new StreamReader(FilePath);
                    string MailText = str.ReadToEnd();
                    str.Close();




                    var domain = _config.GetSection("FrontendDomain").Value;
                    MailText = MailText.Replace("{FrontendDomain}", domain);

                    var builder = new MimeKit.BodyBuilder();

                    MailText = MailText.Replace("{Requester}", emp.GetFullName());
                    MailText = MailText.Replace("{ApproverName}", approver.GetFullName());
                    MailText = MailText.Replace("{Currency}", _context.CurrencyTypes.Find(emp.CurrencyTypeId).CurrencyCode);
                    MailText = MailText.Replace("{RequestedAmount}", CashAdvancereq.CashAdvanceAmount.ToString());
                    MailText = MailText.Replace("{RequestNumber}", CashAdvancereq.Id.ToString());
                    builder.HtmlBody = MailText;

                    EmailDto emailDto = new EmailDto();
                    emailDto.To = approverMailAddress;
                    emailDto.Subject = subject;
                    emailDto.Body = builder.HtmlBody;

                    await _emailSender.SendEmailAsync(emailDto);
                    _logger.LogInformation(approver.GetFullName() + " Email Sent");

                    IsFirstEmail = false;
                }
            }
            //_context.Entry(CashAdvanceRequest).State = EntityState.Modified;

            //Step-3 change the Disbursements and Claims Master record

            var disburseMasterRecord = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == CashAdvanceRequestDto.Id && d.RequestTypeId == (int)ERequestType.CashAdvance).FirstOrDefault();
            // disburseMasterRecord.BusinessTypeId = newBusinesTypeId;
            //disburseMasterRecord.BusinessUnitId = newBusinesUnitId;

            disburseMasterRecord.CostCenterId = costcentreId;
            disburseMasterRecord.ProjectId = newProjId;
            disburseMasterRecord.SubProjectId = newSubProjId;
            disburseMasterRecord.WorkTaskId = newWorkTaskId;
            disburseMasterRecord.RecordDate = DateTime.UtcNow;
            disburseMasterRecord.ClaimAmount = CashAdvanceRequestDto.CashAdvanceAmount;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DisbursementsAndClaimsMasters update failed ");
            }
            _logger.LogInformation("DisbursementsAndClaimsMaster table update complete");
            _logger.LogInformation("Cash Advance Request updated successfully");



            return Ok(new RespStatus { Status = "Success", Message = "Request Updated!" });
        }

        // POST: api/CashAdvanceRequests
        [HttpPost]
        [ActionName("PostCashAdvanceRequest")]
        public async Task<ActionResult<CashAdvanceRequest>> PostCashAdvanceRequest(CashAdvanceRequestDTO CashAdvanceRequestDto)
        {
            ReturnIntAndResponseString SuccessResult = null;

            if (CashAdvanceRequestDto == null || (CashAdvanceRequestDto.BusinessUnitId == null && CashAdvanceRequestDto.ProjectId == null) || (CashAdvanceRequestDto.BusinessUnitId == 0 && CashAdvanceRequestDto.ProjectId == 0))
            {
                _logger.LogError("PostCashAdvanceRequestDto - null request data");
                return Conflict(new RespStatus { Status = "Failure", Message = "CashAdvance Request invalid!" });
            }

            if (CashAdvanceRequestDto.CashAdvanceAmount <= 0 )
            {
                _logger.LogError("PostCashAdvanceRequestDto - Invalid Cash Advance Amount " + CashAdvanceRequestDto.CashAdvanceAmount );
                return Conflict(new RespStatus { Status = "Failure", Message = "Invalid Cash Advance Amount " + CashAdvanceRequestDto.CashAdvanceAmount });
            }

            /*!!=========================================
               Check Eligibility for Cash Disbursement
             .==========================================*/

            Double? empCurAvailBal = GetEmpCurrentAvailableCashAdvanceBalance(CashAdvanceRequestDto);
            Double? empCurMaxLimit = GetEmpCurrentMaxCashBorrowLimit(CashAdvanceRequestDto);
            //Check any pending CashAdvance requests for employee, if then total them all to find the amount eligible






            double? pendingPettCashRequestAmounts = _context.CashAdvanceRequests.Where(p => p.EmployeeId == CashAdvanceRequestDto.EmployeeId && p.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).Select(s => s.CashAdvanceAmount).Sum();

            if (pendingPettCashRequestAmounts + CashAdvanceRequestDto.CashAdvanceAmount <= empCurAvailBal
                || CashAdvanceRequestDto.CashAdvanceAmount <= empCurAvailBal
                && CashAdvanceRequestDto.CashAdvanceAmount > 0
                || pendingPettCashRequestAmounts + CashAdvanceRequestDto.CashAdvanceAmount <= empCurMaxLimit)
            {
                if (CashAdvanceRequestDto.ProjectId != null)
                {
                    SuccessResult = await Task.Run(() => ProjectCashRequest(CashAdvanceRequestDto, empCurAvailBal));
                }
                else
                {
                    SuccessResult = await Task.Run(() => BusinessUnitCashRequest(CashAdvanceRequestDto, empCurAvailBal));
                }

            }
            else
            {
                return Conflict(new RespStatus() { Status = "Failure", Message = "Invalid Cash Request Amount Or Limit Exceeded" });
            }

            if (SuccessResult.IntReturn == 0)
            {
                _logger.LogInformation("Cash Advance Request - Process completed");

                return Created("PostCashAdvanceRequest", new RespStatus() { Status = "Success", Message = SuccessResult.StrResponse });
            }
            else
            {
                _logger.LogError("Cash Advance Request creation failed -Check approval Role Map assignment!");

                return BadRequest(new RespStatus { Status = "Failure", Message = SuccessResult.StrResponse });
            }

        }

        // DELETE: api/CashAdvanceRequests/5
        [HttpDelete("{id}")]
        [ActionName("DeleteCashAdvanceRequest")]

        public async Task<IActionResult> DeleteCashAdvanceRequest(int id)
        {
            var CashAdvanceRequest = await _context.CashAdvanceRequests.FindAsync(id);
            if (CashAdvanceRequest == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Cash Advance Request Id Invalid!" });
            }

            var ClmApprvStatusTrackers = _context.CashAdvanceStatusTrackers.Where(c => c.CashAdvanceRequestId == CashAdvanceRequest.Id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Approved);

            int ApprovedCount = ClmApprvStatusTrackers.Count();

            if (ApprovedCount > 0)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Cash Advance Request cant be Deleted after Approval!" });
            }


            //update the EmpCashAdvanceBalance to credit back the deducted amount
            EmpCurrentCashAdvanceBalance empCashAdvanceBal = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == CashAdvanceRequest.EmployeeId).FirstOrDefault();
            empCashAdvanceBal.CurBalance += CashAdvanceRequest.CashAdvanceAmount;
            empCashAdvanceBal.UpdatedOn = DateTime.UtcNow;
            _context.EmpCurrentCashAdvanceBalances.Update(empCashAdvanceBal);

            _context.CashAdvanceRequests.Remove(CashAdvanceRequest);

            var ClaimApprStatusTrackers = _context.CashAdvanceStatusTrackers.Where(c => c.CashAdvanceRequestId == CashAdvanceRequest.Id).ToList();

            foreach (var claim in ClaimApprStatusTrackers)
            {
                _context.CashAdvanceStatusTrackers.Remove(claim);
            }

            var disburseAndClaims = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == CashAdvanceRequest.Id && d.RequestTypeId == (int)ERequestType.CashAdvance).ToList();
            foreach (var disburse in disburseAndClaims)
            {
                _context.DisbursementsAndClaimsMasters.Remove(disburse);
            }
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Cash Advance Request Deleted!" });
        }


        private Double? GetEmpCurrentAvailableCashAdvanceBalance(CashAdvanceRequestDTO CashAdvanceRequest)
        {

            var empCurPettyBalance = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == CashAdvanceRequest.EmployeeId).FirstOrDefault();

            if (empCurPettyBalance != null)
            {
                return empCurPettyBalance.CurBalance;
            }

            AddEmpCurrentCashAdvanceBalanceForEmployee(CashAdvanceRequest.EmployeeId);


            return 0;
        }

        private Double? GetEmpCurrentMaxCashBorrowLimit(CashAdvanceRequestDTO CashAdvanceRequest)
        {
            Employee emp = _context.Employees.Find(CashAdvanceRequest.EmployeeId);

            var empMaxLimit = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == CashAdvanceRequest.EmployeeId).FirstOrDefault().MaxCashAdvanceLimit;

            if (empMaxLimit != 0)
            {
                return empMaxLimit;
            }

            return 0;
        }


        /// <summary>
        /// This is the option 1 : : PROJECT BASED CASH ADVANCE REQUEST
        /// </summary>
        /// <param name="CashAdvanceRequestDto"></param>
        /// <param name="empCurAvailBal"></param>
        private async Task<ReturnIntAndResponseString> ProjectCashRequest(CashAdvanceRequestDTO CashAdvanceRequestDto, Double? empCurAvailBal)
        {

            //### 1. If Employee Eligible for Cash Claim enter a record and reduce the available amount for next claim
            ReturnIntAndResponseString returnIntAndResponseString = new();

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {

                int costCenterId = _context.Projects.Find(CashAdvanceRequestDto.ProjectId).CostCenterId;
                int? projManagerid = _context.Projects.Find(CashAdvanceRequestDto.ProjectId).ProjectManagerId;
                var approver = _context.Employees.Find(projManagerid);
                int reqEmpid = CashAdvanceRequestDto.EmployeeId;
                bool isSelfApprovedRequest = false;
                Employee reqEmp = _context.Employees.Find(reqEmpid);

                #region
                if (approver != null)
                {
                    _logger.LogInformation("Project Manager defined, no issues");
                }
                else
                {
                    _logger.LogError("Project Manager is not Assigned");
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Project Manager is not Assigned";
                    return returnIntAndResponseString;
                }
                ////

                Double? empReqAmount = CashAdvanceRequestDto.CashAdvanceAmount;

                EmpCurrentCashAdvanceBalance empcurCashAdvanceBal = _context.EmpCurrentCashAdvanceBalances.Where(x => x.EmployeeId == reqEmpid).FirstOrDefault();

                double? maxCashAllowedForRole = empcurCashAdvanceBal.MaxCashAdvanceLimit;//int empApprGroupId = _context.Employees.Find(empid).ApprovalGroupId;

                if (CashAdvanceRequestDto.CashAdvanceAmount > maxCashAllowedForRole)
                {
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Cash Advance Limit exceeds Max Limit";
                    return returnIntAndResponseString;
                }

                var curCashAdvanceBal = _context.EmpCurrentCashAdvanceBalances.Where(x => x.EmployeeId == reqEmpid).FirstOrDefault();
                curCashAdvanceBal.Id = curCashAdvanceBal.Id;
                curCashAdvanceBal.CurBalance = empCurAvailBal - empReqAmount <= maxCashAllowedForRole ? empCurAvailBal - empReqAmount : maxCashAllowedForRole;
                curCashAdvanceBal.EmployeeId = reqEmpid;
                curCashAdvanceBal.UpdatedOn = DateTime.UtcNow;
                _context.Update(curCashAdvanceBal);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Project: CashAdvanceRequests");
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Cash Advance CashAdvance Balance Update failed";
                    return returnIntAndResponseString;
                }

                #endregion

                //##### 2. Adding entry to CashAdvanceRequest table for record
                #region
                var pcrq = new CashAdvanceRequest()
                {
                    EmployeeId = reqEmpid,
                    CashAdvanceAmount = empReqAmount,
                    RequestDate = DateTime.UtcNow,
                    BusinessTypeId = null, //project
                    BusinessUnitId = null, //project
                    ProjectId = CashAdvanceRequestDto.ProjectId,
                    CostCenterId = costCenterId,
                    SubProjectId = CashAdvanceRequestDto.SubProjectId,
                    WorkTaskId = CashAdvanceRequestDto.WorkTaskId,
                    CashAdvanceRequestDesc = CashAdvanceRequestDto.CashAdvanceRequestDesc,
                    CurrencyTypeId = CashAdvanceRequestDto.CurrencyTypeId,
                    ApprovalStatusTypeId = (int)EApprovalStatus.Pending,
                    Comments = "Cash Advance Request in Process!"

                };
                _context.CashAdvanceRequests.Add(pcrq);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Project: CashAdvanceRequests");

                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Project: CashAdvance Request creation failed";
                    return returnIntAndResponseString;
                }

                CashAdvanceRequestDto.Id = pcrq.Id;
                #endregion

                //##### 3. Add an entry to CashAdvance Status tracker
                //get costcenterID based on project
                #region

                ///////////////////////////// Check if self Approved Request /////////////////////////////
                //int? maxApprLevel = _context.ApprovalRoleMaps.Max(a => a.ApprovalLevelId);
                //int reqApprLevel = _context.ApprovalRoleMaps.Where(a => a.JobRoleId == _context.Employees.Find(empid).JobRoleId).FirstOrDefault().Id;


                //if highest approver is requesting Cash Advance request himself
                if (projManagerid == reqEmpid)
                {
                    isSelfApprovedRequest = true;
                }
                //////////////////////////////////////////////////////////////////////////////////////////
                if (isSelfApprovedRequest)
                {
                    CashAdvanceStatusTracker claimAppStatusTrack = new()
                    {
                        EmployeeId = reqEmpid,
                        CashAdvanceRequestId = CashAdvanceRequestDto.Id,
                        BusinessTypeId = null, //project
                        BusinessUnitId = null, //project
                        ProjectId = CashAdvanceRequestDto.ProjectId,
                        SubProjectId = CashAdvanceRequestDto.SubProjectId,
                        WorkTaskId = CashAdvanceRequestDto.WorkTaskId,
                        JobRoleId = null,
                        ApprovalGroupId = null,
                        ApprovalLevelId = 2, //empApprLevel or 2 default approval level is 2 for Project based request
                        RequestDate = DateTime.UtcNow,
                        ApproverEmpId = reqEmpid,
                        ProjManagerId = projManagerid,
                        ApproverActionDate = DateTime.UtcNow,
                        ApprovalStatusTypeId = (int)EApprovalStatus.Approved, //1-Initiating, 2-Pending, 3-InReview, 4-Approved, 5-Rejected
                        Comments = "Self Approved Request!"
                    };


                    _context.CashAdvanceStatusTrackers.Add(claimAppStatusTrack);
                    pcrq.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                    pcrq.Comments = "Approved";
                    _context.CashAdvanceRequests.Update(pcrq);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    CashAdvanceStatusTracker claimAppStatusTrack = new()
                    {
                        EmployeeId = reqEmpid,
                        CashAdvanceRequestId = CashAdvanceRequestDto.Id,
                        BusinessTypeId = null, //project
                        BusinessUnitId = null, //project
                     
                        ProjectId = CashAdvanceRequestDto.ProjectId,
                        SubProjectId = CashAdvanceRequestDto.SubProjectId,
                        WorkTaskId = CashAdvanceRequestDto.WorkTaskId,
                        JobRoleId = null,
                        // get the next ProjectManager approval.
                        ApprovalGroupId = null,
                        ApprovalLevelId = 2, // default approval level is 2 for Project based request
                        RequestDate = DateTime.UtcNow,
                        ApproverEmpId = null,
                        ProjManagerId = projManagerid,
                        ApproverActionDate = null,
                        ApprovalStatusTypeId = (int)EApprovalStatus.Pending, //1-Initiating, 2-Pending, 3-InReview, 4-Approved, 5-Rejected
                        Comments = "Awaiting Approver Action"
                    };


                    _context.CashAdvanceStatusTrackers.Add(claimAppStatusTrack);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Project: CashAdvanceRequests");

                        returnIntAndResponseString.IntReturn = 1;
                        returnIntAndResponseString.StrResponse = "Project: CashAdvance Status Tracker creation failed";
                        return returnIntAndResponseString;
                    }
                    #endregion


                    //##### 4. Send email to the user
                    //####################################
                    #region
                    _logger.LogInformation("Project: CashAdvance Email Start");

                    string[] paths = { Directory.GetCurrentDirectory(), "CashAdvanceApprNotificationEmail.html" };
                    string FilePath = Path.Combine(paths);
                    _logger.LogInformation("Email template path " + FilePath);
                    StreamReader str = new StreamReader(FilePath);
                    string MailText = str.ReadToEnd();
                    str.Close();

                    var approverMailAddress = approver.Email;
                    string subject = "CashAdvance Request Approval " + CashAdvanceRequestDto.Id.ToString();
                    Employee emp = _context.Employees.Find(CashAdvanceRequestDto.EmployeeId);
                    var CashAdvancereq = _context.CashAdvanceRequests.Find(CashAdvanceRequestDto.Id);

                    var domain = _config.GetSection("FrontendDomain").Value;
                    MailText = MailText.Replace("{FrontendDomain}", domain);

                    var builder = new MimeKit.BodyBuilder();

                    MailText = MailText.Replace("{Requester}", emp.GetFullName());
                    MailText = MailText.Replace("{ApproverName}", approver.GetFullName());
                    MailText = MailText.Replace("{Currency}", _context.CurrencyTypes.Find(emp.CurrencyTypeId).CurrencyCode);
                    MailText = MailText.Replace("{RequestedAmount}", CashAdvancereq.CashAdvanceAmount.ToString());
                    MailText = MailText.Replace("{RequestNumber}", CashAdvancereq.Id.ToString());
                    builder.HtmlBody = MailText;

                    EmailDto emailDto = new EmailDto();
                    emailDto.To = approverMailAddress;
                    emailDto.Subject = subject;
                    emailDto.Body = builder.HtmlBody;
                    

                     await _emailSender.SendEmailAsync(emailDto);
                    _logger.LogInformation("Project: CashAdvance Email Sent");
                    #endregion
                }



                //##### 5. Adding a entry in DisbursementsAndClaimsMaster table for records
                #region
                _logger.LogInformation("Project: Disbursement table insert start");
                DisbursementsAndClaimsMaster disbursementsAndClaimsMaster = new();

                disbursementsAndClaimsMaster.EmployeeId = reqEmpid;
                disbursementsAndClaimsMaster.BlendedRequestId = CashAdvanceRequestDto.Id;
                disbursementsAndClaimsMaster.BusinessTypeId = null; //project
                disbursementsAndClaimsMaster.BusinessUnitId = null; //project
                disbursementsAndClaimsMaster.RequestTypeId = (int)ERequestType.CashAdvance;
                disbursementsAndClaimsMaster.ProjectId = CashAdvanceRequestDto.ProjectId;
                disbursementsAndClaimsMaster.SubProjectId = CashAdvanceRequestDto.SubProjectId;
                disbursementsAndClaimsMaster.WorkTaskId = CashAdvanceRequestDto.WorkTaskId;
                disbursementsAndClaimsMaster.RecordDate = DateTime.UtcNow;
                disbursementsAndClaimsMaster.CurrencyTypeId = CashAdvanceRequestDto.CurrencyTypeId;
                disbursementsAndClaimsMaster.ClaimAmount = CashAdvanceRequestDto.CashAdvanceAmount;
                disbursementsAndClaimsMaster.AmountToWallet = 0;
                disbursementsAndClaimsMaster.AmountToCredit = isSelfApprovedRequest ? CashAdvanceRequestDto.CashAdvanceAmount : 0;
                disbursementsAndClaimsMaster.IsSettledAmountCredited = false;
                disbursementsAndClaimsMaster.CostCenterId = costCenterId;
                disbursementsAndClaimsMaster.ApprovalStatusId = isSelfApprovedRequest ? (int)EApprovalStatus.Approved : (int)EApprovalStatus.Pending; //1-Initiating, 2-Pending, 3-InReview, 4-Approved, 5-Rejected

                _context.DisbursementsAndClaimsMasters.Add(disbursementsAndClaimsMaster);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    string error = ex.Message;

                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Project: DisbursementsAndClaims reacord creation failed";
                    return returnIntAndResponseString;
                }
                #endregion
                _context.Update(disbursementsAndClaimsMaster);
                await _context.SaveChangesAsync();
                await AtoCashDbContextTransaction.CommitAsync();
            }
            returnIntAndResponseString.IntReturn = 0;
            returnIntAndResponseString.StrResponse = "Project: DisbursementsAndClaims reacord created";
            return returnIntAndResponseString;

        }

        /// <summary>
        /// This is option 2 : Business Unit BASED CASH ADVANCE REQUEST
        /// </summary>
        /// <param name="CashAdvanceRequestDto"></param>
        /// <param name="empCurAvailBal"></param>
        private async Task<ReturnIntAndResponseString> BusinessUnitCashRequest(CashAdvanceRequestDTO CashAdvanceRequestDto, Double? empCurAvailBal)
        {
            //### 1. If Employee Eligible for Cash Claim enter a record and reduce the available amount for next claim
            #region
            ReturnIntAndResponseString returnIntAndResponseString = new();
            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {

                int? reqBussUnitId = CashAdvanceRequestDto.BusinessUnitId;
                int costCenterId = _context.BusinessUnits.Find(reqBussUnitId).CostCenterId ?? 0;
                int reqEmpid = CashAdvanceRequestDto.EmployeeId;
                Employee reqEmp = _context.Employees.Find(reqEmpid);

                if (reqEmp == null)
                {
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Business: Employee Id is Invalid";
                    return returnIntAndResponseString;
                }
                EmployeeExtendedInfo reqEmpExtInfo = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == CashAdvanceRequestDto.EmployeeId && e.BusinessUnitId == reqBussUnitId).FirstOrDefault();

                int? reqJobRoleId = reqEmpExtInfo.JobRoleId;
                int? reqApprGroupId = reqEmpExtInfo.ApprovalGroupId;

                //if Approval Role Map list is null

                var approRoleMap = _context.ApprovalRoleMaps.Include("ApprovalLevel").Where(a => a.ApprovalGroupId == reqApprGroupId).FirstOrDefault();

                if (approRoleMap == null)
                {
                    _logger.LogError("Approver Role Map Not defined, approval group id " + _context.ApprovalGroups.Find(reqApprGroupId).ApprovalGroupCode);
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Approver Role Map Not defined, approval group id " + _context.ApprovalGroups.Find(reqApprGroupId).ApprovalGroupCode;
                    return returnIntAndResponseString;
                }
                else
                {
                    var approRoleMaps = _context.ApprovalRoleMaps.Include("ApprovalLevel").Where(a => a.ApprovalGroupId == reqApprGroupId).ToList();

                    foreach (ApprovalRoleMap ApprMap in approRoleMaps)
                    {
                        int? jobRole_id = ApprMap.JobRoleId;

                        var employeeExtendedInfo = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == jobRole_id && e.ApprovalGroupId == reqApprGroupId).FirstOrDefault();
                        if (employeeExtendedInfo == null)
                        {
                            _logger.LogError("Approver employee not mapped for RoleMap RoleId:" + _context.JobRoles.Find(jobRole_id).JobRoleName + "ApprovalGroupId:" + _context.ApprovalGroups.Find(reqApprGroupId).ApprovalGroupCode);
                            returnIntAndResponseString.IntReturn = 1;
                            returnIntAndResponseString.StrResponse = "Approver employee not mapped for RoleMap RoleId:" + _context.JobRoles.Find(jobRole_id).JobRoleName + "ApprovalGroupId:" + _context.ApprovalGroups.Find(reqApprGroupId).ApprovalGroupCode;
                            return returnIntAndResponseString;
                        }

                        int? approverEmpId = employeeExtendedInfo.EmployeeId;

                        var approver = await _context.Employees.FirstAsync(e => e.Id == approverEmpId);
                        if (approver == null)
                        {
                            _logger.LogError("Approver employee not mapped for RoleMap RoleId:" + _context.JobRoles.Find(jobRole_id).JobRoleName + "ApprovalGroupId:" + _context.ApprovalGroups.Find(reqApprGroupId).ApprovalGroupCode);
                            returnIntAndResponseString.IntReturn = 1;
                            returnIntAndResponseString.StrResponse = "Approver employee not mapped for RoleMap RoleId:" + _context.JobRoles.Find(jobRole_id).JobRoleName + "ApprovalGroupId:" + _context.ApprovalGroups.Find(reqApprGroupId).ApprovalGroupCode;
                            return returnIntAndResponseString;
                        }

                    }
                }
                int? maxApprLevel = _context.ApprovalRoleMaps.Include("ApprovalLevel").Where(a => a.ApprovalGroupId == reqApprGroupId).ToList().Select(x => x.ApprovalLevel).Max(a => a.Level);
                int? reqApprLevel = _context.ApprovalRoleMaps.Include("ApprovalLevel").Where(a => a.ApprovalGroupId == reqApprGroupId && a.JobRoleId == reqJobRoleId).Select(x => x.ApprovalLevel).FirstOrDefault().Level;

                bool isSelfApprovedRequest = false;
                _logger.LogInformation("All Approvers defined");
                ///
                Double? empReqAmount = CashAdvanceRequestDto.CashAdvanceAmount;

                _logger.LogInformation("Business Unit: CashAdvanceRequestDto balance check Start");

                EmpCurrentCashAdvanceBalance empcurCashAdvanceBal = _context.EmpCurrentCashAdvanceBalances.Where(x => x.EmployeeId == reqEmpid).FirstOrDefault();

                var reqMaxCashAdvanceLimit = empcurCashAdvanceBal.MaxCashAdvanceLimit;


                if (reqMaxCashAdvanceLimit >= empCurAvailBal - empReqAmount)
                {
                    empcurCashAdvanceBal.CurBalance = empCurAvailBal - empReqAmount;
                }

                empcurCashAdvanceBal.UpdatedOn = DateTime.UtcNow;
                _context.Update(empcurCashAdvanceBal);
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("BusinessUnit: CashAdvanceRequestDto balance check completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BusinessUnit: CashAdvanceRequest balance check failed");
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "BusinessUnit: CashAdvanceRequest balance check failed";
                    return returnIntAndResponseString;

                }
                ///
                #endregion
                _logger.LogInformation("Business Unit: CashAdvanceRequest insert start");
                //##### 2. Adding entry to CashAdvanceRequest table for record
                #region
                var pcrq = new CashAdvanceRequest()
                {
                    EmployeeId = reqEmpid,
                    CashAdvanceAmount = empReqAmount,
                    RequestDate = DateTime.UtcNow,
                    CashAdvanceRequestDesc = CashAdvanceRequestDto.CashAdvanceRequestDesc,
                    ProjectId = null,
                    SubProjectId = null,
                    WorkTaskId = null,
                    BusinessTypeId = CashAdvanceRequestDto.BusinessTypeId,
                    BusinessUnitId = CashAdvanceRequestDto.BusinessUnitId,
                    CostCenterId = costCenterId,
                    CurrencyTypeId = CashAdvanceRequestDto.CurrencyTypeId,
                    ApprovalStatusTypeId = (int)EApprovalStatus.Pending,
                    Comments = "Cash Advance Request in Process!"

                };
                _context.CashAdvanceRequests.Add(pcrq);

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("BusinessUnit: CashAdvanceRequest insert success");
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "BusinessUnit: CashAdvanceRequest insert failed");
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "BusinessUnit: CashAdvanceRequest insert failed";
                    return returnIntAndResponseString;
                }



                //get the saved record Id
                CashAdvanceRequestDto.Id = pcrq.Id;

                #endregion

                //##### STEP 3. ClaimsApprovalTracker to be updated for all the allowed Approvers


                ///////////////////////////// Check if self Approved Request /////////////////////////////

                //if highest approver is requesting Cash Advance request himself
                if (maxApprLevel == reqApprLevel)
                {
                    isSelfApprovedRequest = true;
                }
                //////////////////////////////////////////////////////////////////////////////////////////


                var getEmpClaimApproversAllLevels = _context.ApprovalRoleMaps
                                    .Include(a => a.ApprovalLevel)
                                    .Where(a => a.ApprovalGroupId == reqApprGroupId)
                                    .OrderBy(o => o.ApprovalLevel.Level).ToList();
                bool isFirstApprover = true;
                _logger.LogInformation("Business Unit: CashAdvanceRequest status tracker insert start");
                if (isSelfApprovedRequest)
                {

                    CashAdvanceStatusTracker claimAppStatusTrack = new()
                    {
                        EmployeeId = CashAdvanceRequestDto.EmployeeId,
                        CashAdvanceRequestId = CashAdvanceRequestDto.Id,
                        BusinessTypeId = CashAdvanceRequestDto.BusinessTypeId,
                        BusinessUnitId = CashAdvanceRequestDto.BusinessUnitId,
                        ProjectId = null,
                        SubProjectId = null,
                        WorkTaskId = null,
                        JobRoleId = reqJobRoleId,
                        ApprovalGroupId = reqApprGroupId,
                        ApprovalLevelId = reqApprLevel,
                        RequestDate = DateTime.UtcNow,
                        ApproverEmpId = reqEmpid,
                        ApproverActionDate = DateTime.UtcNow,
                        ApprovalStatusTypeId = (int)EApprovalStatus.Approved,
                        Comments = "Self Approved Request!"
                        //1-Initiating, 2-Pending, 3-InReview, 4-Approved, 5-Rejected
                    };
                    _context.CashAdvanceStatusTrackers.Add(claimAppStatusTrack);
                    pcrq.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                    pcrq.Comments = "Approved";


                    _context.CashAdvanceRequests.Update(pcrq);

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Business Unit: CashAdvanceRequest status tracker insert start");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Business Unit: CashAdvanceRequest status tracker insert failed");

                        returnIntAndResponseString.IntReturn = 1;
                        returnIntAndResponseString.StrResponse = "Business Unit: CashAdvanceRequest status tracker insert failed";
                        return returnIntAndResponseString;
                    }

                }
                else
                {
                    foreach (ApprovalRoleMap ApprMap in getEmpClaimApproversAllLevels)
                    {

                        int? apprjobRoleId = ApprMap.JobRoleId;

                        int? approverEmpId = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == apprjobRoleId && e.ApprovalGroupId == reqApprGroupId).FirstOrDefault().EmployeeId;

                        var approver = await _context.Employees.FirstAsync(e => e.Id == approverEmpId);


                        if (approverEmpId == null || approverEmpId == 0)
                        {
                            continue;
                        }
                        int? approverLevelid = _context.ApprovalRoleMaps.Where(x => x.JobRoleId == apprjobRoleId && x.ApprovalGroupId == reqApprGroupId).FirstOrDefault().ApprovalLevelId;
                        int? approverLevel = _context.ApprovalLevels.Find(approverLevelid).Level;

                        if (reqApprLevel >= approverLevel)
                        {
                            continue;
                        }


                        CashAdvanceStatusTracker claimAppStatusTrack = new()
                        {
                            EmployeeId = CashAdvanceRequestDto.EmployeeId,
                            CashAdvanceRequestId = CashAdvanceRequestDto.Id,
                            BusinessTypeId = CashAdvanceRequestDto.BusinessTypeId,
                            BusinessUnitId = CashAdvanceRequestDto.BusinessUnitId,
                            ProjManagerId = null,
                            ProjectId = null,
                            SubProjectId = null,
                            WorkTaskId = null,
                            JobRoleId = apprjobRoleId,
                            ApprovalGroupId = reqApprGroupId,
                            ApprovalLevelId = ApprMap.ApprovalLevelId,
                            RequestDate = DateTime.UtcNow,
                            ApproverEmpId = null,
                            ApproverActionDate = null,
                            ApprovalStatusTypeId = isFirstApprover ? (int)EApprovalStatus.Pending : (int)EApprovalStatus.Intitated,
                            Comments = "Awaiting Approver Action"
                            //1-Initiating, 2-Pending, 3-InReview, 4-Approved, 5-Rejected
                        };


                        _context.CashAdvanceStatusTrackers.Add(claimAppStatusTrack);

                        await _context.SaveChangesAsync();



                        if (isFirstApprover)
                        {
                            //##### 4. Send email to the Approver
                            //####################################
                            _logger.LogInformation(approver.GetFullName() + " Email Start");

                            string[] paths = { Directory.GetCurrentDirectory(), "CashAdvanceApprNotificationEmail.html" };
                            string FilePath = Path.Combine(paths);
                            _logger.LogInformation("Email template path " + FilePath);
                            StreamReader str = new StreamReader(FilePath);
                            string MailText = str.ReadToEnd();
                            str.Close();

                            var approverMailAddress = approver.Email;
                            string subject = "CashAdvance Request Approval " + CashAdvanceRequestDto.Id.ToString();
                            Employee emp = _context.Employees.Find(CashAdvanceRequestDto.EmployeeId);
                            var CashAdvancereq = _context.CashAdvanceRequests.Find(CashAdvanceRequestDto.Id);

                            var domain = _config.GetSection("FrontendDomain").Value;
                            MailText = MailText.Replace("{FrontendDomain}", domain);

                            var builder = new MimeKit.BodyBuilder();

                            MailText = MailText.Replace("{Requester}", emp.GetFullName());
                            MailText = MailText.Replace("{ApproverName}", approver.GetFullName());
                            MailText = MailText.Replace("{Currency}", _context.CurrencyTypes.Find(emp.CurrencyTypeId).CurrencyCode);
                            MailText = MailText.Replace("{RequestedAmount}", CashAdvancereq.CashAdvanceAmount.ToString());
                            MailText = MailText.Replace("{RequestNumber}", CashAdvancereq.Id.ToString());
                            builder.HtmlBody = MailText;

                            EmailDto emailDto = new EmailDto();
                            emailDto.To = approverMailAddress;
                            emailDto.Subject = subject;
                            emailDto.Body = builder.HtmlBody;

                            await _emailSender.SendEmailAsync(emailDto);
                            _logger.LogInformation(approver.GetFullName() + " Email Sent");

                        }

                        //first approver will be added as Pending, other approvers will be with In Approval Queue
                        isFirstApprover = false;

                    }

                }

                //##### STEP 5. Adding a SINGLE entry in DisbursementsAndClaimsMaster table for records
                #region
                DisbursementsAndClaimsMaster disbursementsAndClaimsMaster = new();

                disbursementsAndClaimsMaster.EmployeeId = reqEmpid;
                disbursementsAndClaimsMaster.BlendedRequestId = pcrq.Id;
                disbursementsAndClaimsMaster.RequestTypeId = (int)ERequestType.CashAdvance;
                disbursementsAndClaimsMaster.BusinessTypeId = CashAdvanceRequestDto.BusinessTypeId;
                disbursementsAndClaimsMaster.BusinessUnitId = CashAdvanceRequestDto.BusinessUnitId;
                disbursementsAndClaimsMaster.ProjectId = null;
                disbursementsAndClaimsMaster.SubProjectId = null;
                disbursementsAndClaimsMaster.WorkTaskId = null;
                disbursementsAndClaimsMaster.RecordDate = DateTime.UtcNow;
                disbursementsAndClaimsMaster.CurrencyTypeId = CashAdvanceRequestDto.CurrencyTypeId;
                disbursementsAndClaimsMaster.ClaimAmount = empReqAmount;
                disbursementsAndClaimsMaster.AmountToCredit = isSelfApprovedRequest ? empReqAmount : 0;
                disbursementsAndClaimsMaster.IsSettledAmountCredited = false;
                disbursementsAndClaimsMaster.CostCenterId = costCenterId;
                disbursementsAndClaimsMaster.ApprovalStatusId = isSelfApprovedRequest ? (int)EApprovalStatus.Approved : (int)EApprovalStatus.Pending;

                _context.DisbursementsAndClaimsMasters.Add(disbursementsAndClaimsMaster);
                try
                {

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Business Unit: Disbursmenent record insert failed");
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Business Unit: Disbursmenent record insert failed";
                    return returnIntAndResponseString;
                }
                #endregion
                await AtoCashDbContextTransaction.CommitAsync();
            }
            returnIntAndResponseString.IntReturn = 0;
            returnIntAndResponseString.StrResponse = "Business Unit: Cash Request Created";
            return returnIntAndResponseString;
        }


        private void AddEmpCurrentCashAdvanceBalanceForEmployee(int id)
        {
            if (id == 0)
            {
                return;
            }

            var emp = _context.Employees.Find(id);

            if (emp != null)
            {
                Double? empCashAdvanceAmountEligible = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == id).FirstOrDefault().MaxCashAdvanceLimit;
                _context.EmpCurrentCashAdvanceBalances.Add(new EmpCurrentCashAdvanceBalance()
                {
                    EmployeeId = id,
                    CurBalance = empCashAdvanceAmountEligible,
                    UpdatedOn = DateTime.UtcNow
                });

                _context.SaveChangesAsync();
            }
            return;

        }
















    }

}
