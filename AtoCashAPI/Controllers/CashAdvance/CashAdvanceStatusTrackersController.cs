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
using EmailService;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace AtoCashAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User, Manager")]
    public class CashAdvanceStatusTrackersController : ControllerBase
    {
        private readonly AtoCashDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<CashAdvanceStatusTrackersController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public CashAdvanceStatusTrackersController(AtoCashDbContext context, IEmailSender emailSender,
                                                        ILogger<CashAdvanceStatusTrackersController> logger,
                                                        UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        // GET: api/CashAdvanceStatusTrackers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CashAdvanceStatusTrackerDTO>>> GetCashAdvanceStatusTrackers()
        {
            List<CashAdvanceStatusTrackerDTO> ListCashAdvanceStatusTrackerDTO = new();

            var CashAdvanceStatusTrackers = await _context.CashAdvanceStatusTrackers.ToListAsync();

            foreach (CashAdvanceStatusTracker CashAdvanceStatusTracker in CashAdvanceStatusTrackers)
            {
                CashAdvanceStatusTrackerDTO CashAdvanceStatusTrackerDTO = new()
                {
                    Id = CashAdvanceStatusTracker.Id,
                    EmployeeId = CashAdvanceStatusTracker.EmployeeId,
                    EmployeeName = _context.Employees.Find(CashAdvanceStatusTracker.EmployeeId).GetFullName(),
                    CashAdvanceRequestId = CashAdvanceStatusTracker.CashAdvanceRequestId,
                    BusinessTypeId = CashAdvanceStatusTracker.BusinessTypeId,
                    BusinessType = CashAdvanceStatusTracker.BusinessTypeId != null ? _context.BusinessTypes.Find(CashAdvanceStatusTracker.BusinessTypeId).BusinessTypeName : null,
                    BusinessUnitId = CashAdvanceStatusTracker.BusinessUnitId,
                    BusinessUnit = CashAdvanceStatusTracker.BusinessUnitId != null ? _context.BusinessUnits.Find(CashAdvanceStatusTracker.BusinessUnitId).GetBusinessUnitName() : null,
                    ProjectId = CashAdvanceStatusTracker.ProjectId,
                    ProjectName = CashAdvanceStatusTracker.ProjectId != null ? _context.Projects.Find(CashAdvanceStatusTracker.ProjectId).ProjectName : null,
                    JobRoleId = CashAdvanceStatusTracker.JobRoleId,
                    JobRole = _context.JobRoles.Find(CashAdvanceStatusTracker.JobRoleId).GetJobRole(),
                    ApprovalLevelId = CashAdvanceStatusTracker.ApprovalLevelId,
                    RequestDate = CashAdvanceStatusTracker.RequestDate,
                    ApproverActionDate = CashAdvanceStatusTracker.ApproverActionDate,
                    ApprovalStatusTypeId = CashAdvanceStatusTracker.ApprovalStatusTypeId,
                    ApprovalStatusType = _context.ApprovalStatusTypes.Find(CashAdvanceStatusTracker.ApprovalStatusTypeId).Status,
                    Comments = CashAdvanceStatusTracker.Comments

                };

                ListCashAdvanceStatusTrackerDTO.Add(CashAdvanceStatusTrackerDTO);

            }

            return ListCashAdvanceStatusTrackerDTO.OrderByDescending(o => o.RequestDate).ToList();
        }

        // GET: api/CashAdvanceStatusTrackers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CashAdvanceStatusTrackerDTO>> GetCashAdvanceStatusTracker(int id)
        {


            var CashAdvanceStatusTracker = await _context.CashAdvanceStatusTrackers.FindAsync(id);

            if (CashAdvanceStatusTracker == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Claim Approval Id is Invalid!" });
            }

            CashAdvanceStatusTrackerDTO CashAdvanceStatusTrackerDTO = new()
            {
                Id = CashAdvanceStatusTracker.Id,
                EmployeeId = CashAdvanceStatusTracker.EmployeeId,
                EmployeeName = _context.Employees.Find(CashAdvanceStatusTracker.EmployeeId).GetFullName(),
                CashAdvanceRequestId = CashAdvanceStatusTracker.CashAdvanceRequestId,
                BusinessTypeId = CashAdvanceStatusTracker.BusinessTypeId,
                BusinessType = CashAdvanceStatusTracker.BusinessTypeId != null ? _context.BusinessTypes.Find(CashAdvanceStatusTracker.BusinessTypeId).BusinessTypeName : null,
                BusinessUnitId = CashAdvanceStatusTracker.BusinessUnitId,
                BusinessUnit = CashAdvanceStatusTracker.BusinessUnitId != null ? _context.BusinessUnits.Find(CashAdvanceStatusTracker.BusinessUnitId).GetBusinessUnitName() : null,
                ProjectId = CashAdvanceStatusTracker.ProjectId,
                ProjectName = CashAdvanceStatusTracker.ProjectId != null ? _context.Projects.Find(CashAdvanceStatusTracker.ProjectId).ProjectName : null,
                JobRoleId = CashAdvanceStatusTracker.JobRoleId,
                JobRole = _context.JobRoles.Find(CashAdvanceStatusTracker.JobRoleId).GetJobRole(),
                ApprovalLevelId = CashAdvanceStatusTracker.ApprovalLevelId,
                RequestDate = CashAdvanceStatusTracker.RequestDate,
                ApproverActionDate = CashAdvanceStatusTracker.ApproverActionDate,
                ApprovalStatusTypeId = CashAdvanceStatusTracker.ApprovalStatusTypeId,
                ApprovalStatusType = _context.ApprovalStatusTypes.Find(CashAdvanceStatusTracker.ApprovalStatusTypeId).Status,
                Comments = CashAdvanceStatusTracker.Comments
            };


            return CashAdvanceStatusTrackerDTO;
        }

        /// <summary>
        /// Approver Approving the claim
        /// </summary>
        /// <param name="id"></param>
        /// <param name="CashAdvanceStatusTrackerDto"></param>
        /// <returns></returns>

        // PUT: api/CashAdvanceStatusTrackers/5

        [HttpPut]
        //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, Manager")]
        public async Task<IActionResult> PutCashAdvanceStatusTracker(List<CashAdvanceStatusTrackerDTO> ListCashAdvanceStatusTrackerDto)
        {

            if (ListCashAdvanceStatusTrackerDto.Count == 0)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "No Request to Approve!" });
            }


            bool isNextApproverAvailable = true;
            bool bRejectMessage = false;
            //ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {
                foreach (CashAdvanceStatusTrackerDTO CashAdvanceStatusTrackerDto in ListCashAdvanceStatusTrackerDto)
                {
                    var CashAdvanceStatusTracker = await _context.CashAdvanceStatusTrackers.FindAsync(CashAdvanceStatusTrackerDto.Id);

                    //if same status continue to next loop, otherwise process
                    if (CashAdvanceStatusTracker.ApprovalStatusTypeId == CashAdvanceStatusTrackerDto.ApprovalStatusTypeId)
                    {
                        continue;
                    }

                    if (CashAdvanceStatusTrackerDto.ApprovalStatusTypeId == (int)EApprovalStatus.Rejected)
                    {
                        bRejectMessage = true;
                    }

                    CashAdvanceStatusTracker.ApproverActionDate = DateTime.UtcNow;


                    CashAdvanceStatusTracker.ApproverEmpId = int.Parse(HttpContext.User.Claims.First(c => c.Type == "EmployeeId").Value);

                    CashAdvanceStatusTracker.Comments = bRejectMessage ? CashAdvanceStatusTrackerDto.Comments : "Approved";

                    CashAdvanceStatusTracker claimitem;

                    if (CashAdvanceStatusTrackerDto.BusinessUnitId != null)
                    {
                        var employee = await _context.Employees.FindAsync(CashAdvanceStatusTracker.EmployeeId);
                        var ListEmpExtInfo = await _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == employee.Id).ToListAsync();


                        //Check if the record is already approved
                        //if it is not approved then trigger next approver level email & Change the status to approved
                        if (CashAdvanceStatusTrackerDto.ApprovalStatusTypeId == (int)EApprovalStatus.Approved)
                        {
                            //Get the next approval level (get its ID)
                            int qCashAdvanceRequestId = CashAdvanceStatusTrackerDto.CashAdvanceRequestId ?? 0;

                            isNextApproverAvailable = true;

                            int? CurCashAdvanceLevel = _context.ApprovalLevels.Find(CashAdvanceStatusTrackerDto.ApprovalLevelId).Level;
                            int? nextCashAdvanceLevel = CurCashAdvanceLevel + 1;
                            int? qApprovalLevelId;

                            int? apprGroupId = _context.CashAdvanceStatusTrackers.Find(CashAdvanceStatusTrackerDto.Id).ApprovalGroupId;

                            if (_context.ApprovalRoleMaps.Where(a => a.ApprovalGroupId == apprGroupId && a.ApprovalLevelId == nextCashAdvanceLevel).FirstOrDefault() != null)
                            {
                                qApprovalLevelId = _context.ApprovalLevels.Where(x => x.Level == nextCashAdvanceLevel).FirstOrDefault().Id;
                            }
                            else
                            {
                                qApprovalLevelId = _context.ApprovalLevels.Where(x => x.Level == CurCashAdvanceLevel).FirstOrDefault().Id;
                                isNextApproverAvailable = false;
                            }

                            int qApprovalStatusTypeId = isNextApproverAvailable ? (int)EApprovalStatus.Intitated : (int)EApprovalStatus.Pending;

                            //update the next level approver Track request to PENDING (from Initiating) 
                            //if claimitem is not null change the status
                            if (isNextApproverAvailable)
                            {


                                claimitem = _context.CashAdvanceStatusTrackers.Where(c => c.CashAdvanceRequestId == qCashAdvanceRequestId &&
                                    c.ApprovalStatusTypeId == qApprovalStatusTypeId &&
                                     c.ApprovalGroupId == apprGroupId &&
                                    c.ApprovalLevelId == qApprovalLevelId).FirstOrDefault();

                                if (claimitem != null)
                                {
                                    claimitem.ApprovalStatusTypeId = (int)EApprovalStatus.Pending;
                                }

                            }
                            else
                            {
                                //final approver hence update CashAdvanceRequest
                                claimitem = _context.CashAdvanceStatusTrackers.Where(c => c.CashAdvanceRequestId == qCashAdvanceRequestId &&
                                   c.ApprovalStatusTypeId == qApprovalStatusTypeId &&
                                    c.ApprovalGroupId == apprGroupId &&
                                   c.ApprovalLevelId == qApprovalLevelId).FirstOrDefault();
                                //claimitem.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                                claimitem.ApproverActionDate = DateTime.UtcNow;
                                claimitem.ApproverEmpId = int.Parse(HttpContext.User.Claims.First(c => c.Type == "EmployeeId").Value);

                                //final Approver hence updating ExpenseReimburseRequest table
                                var CashAdvanceRequest = _context.CashAdvanceRequests.Find(qCashAdvanceRequestId);



                                CashAdvanceRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                                CashAdvanceRequest.ApproverActionDate = DateTime.UtcNow;
                                CashAdvanceRequest.Comments = bRejectMessage ? CashAdvanceStatusTrackerDto.Comments : "Approved";
                                _context.Update(CashAdvanceRequest);


                                //DisbursementAndClaimsMaster update the record to Approved (ApprovalStatusId
                                int? disbAndClaimItemId = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == claimitem.CashAdvanceRequestId).FirstOrDefault().Id;
                                var disbAndClaimItem = await _context.DisbursementsAndClaimsMasters.FindAsync(disbAndClaimItemId);

                                disbAndClaimItem.ClaimAmount = CashAdvanceRequest.CashAdvanceAmount;
                                disbAndClaimItem.AmountToWallet = 0;
                                disbAndClaimItem.AmountToCredit = bRejectMessage ? 0 : CashAdvanceRequest.CashAdvanceAmount;
                                disbAndClaimItem.IsSettledAmountCredited = false;
                                disbAndClaimItem.ApprovalStatusId = (int)EApprovalStatus.Approved;
                                _context.Update(disbAndClaimItem);
                                await _context.SaveChangesAsync();
                            }

                            //Save to database
                            if (claimitem != null) { _context.Update(claimitem); };
                            await _context.SaveChangesAsync();

                            int? reqApprGroupId = CashAdvanceStatusTrackerDto.ApprovalGroupId;
                            var getEmpClaimApproversAllLevels = _context.ApprovalRoleMaps.Include(a => a.ApprovalLevel).Where(a => a.ApprovalGroupId == reqApprGroupId).OrderBy(o => o.ApprovalLevel.Level).ToList();

                            foreach (var ApprMap in getEmpClaimApproversAllLevels)
                            {

                                //only next level (level + 1) approver is considered here
                                if (ApprMap.ApprovalLevelId == CashAdvanceStatusTracker.ApprovalLevelId + 1)
                                {
                                    int? jobrole_id = ApprMap.JobRoleId;

                                    var apprEmpId = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == jobrole_id && e.ApprovalGroupId == reqApprGroupId).FirstOrDefault().EmployeeId;

                                    var approver = await _context.Employees.FindAsync(apprEmpId);

                                    //##### 4. Send email to the Approver
                                    //####################################

                                    string[] paths = { Directory.GetCurrentDirectory(), "CashAdvanceApprNotificationEmail.html" };
                                    string FilePath = Path.Combine(paths);
                                    _logger.LogInformation("Email template path " + FilePath);
                                    StreamReader str = new StreamReader(FilePath);
                                    string MailText = str.ReadToEnd();
                                    str.Close();

                                    var approverMailAddress = approver.Email;
                                    string subject = "CashAdvance Request Approval " + CashAdvanceStatusTracker.CashAdvanceRequestId.ToString();
                                    Employee emp = await _context.Employees.FindAsync(CashAdvanceStatusTracker.EmployeeId);
                                    var CashAdvancereq = _context.CashAdvanceRequests.Find(CashAdvanceStatusTracker.CashAdvanceRequestId);

                                    var builder = new MimeKit.BodyBuilder();

                                    MailText = MailText.Replace("{Requester}", emp.GetFullName());
                                    MailText = MailText.Replace("{ApproverName}", approver.GetFullName());
                                    MailText = MailText.Replace("{Currency}", _context.CurrencyTypes.Find(emp.CurrencyTypeId).CurrencyCode);
                                    MailText = MailText.Replace("{RequestedAmount}", CashAdvancereq.CashAdvanceAmount.ToString());
                                    MailText = MailText.Replace("{RequestNumber}", CashAdvancereq.Id.ToString());
                                    builder.HtmlBody = MailText;

                                    var messagemail = new Message(new string[] { approverMailAddress }, subject, builder.HtmlBody);

                                    await _emailSender.SendEmailAsync(messagemail);

                                    break;

                                }
                            }
                        }

                        //if nothing else then just update the approval status
                        CashAdvanceStatusTracker.ApprovalStatusTypeId = CashAdvanceStatusTrackerDto.ApprovalStatusTypeId;


                        int pendingApprovals = _context.CashAdvanceStatusTrackers
                                  .Where(t => t.CashAdvanceRequestId == CashAdvanceStatusTrackerDto.CashAdvanceRequestId &&
                                  t.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).Count();

                        if (pendingApprovals == 0)
                        {
                            var CashAdvanceReq = _context.CashAdvanceRequests.Where(p => p.Id == CashAdvanceStatusTrackerDto.CashAdvanceRequestId).FirstOrDefault();
                            CashAdvanceReq.ApprovalStatusTypeId = CashAdvanceStatusTrackerDto.ApprovalStatusTypeId ?? 0;
                            CashAdvanceReq.ApproverActionDate = DateTime.UtcNow;
                            CashAdvanceReq.Comments = bRejectMessage ? CashAdvanceStatusTrackerDto.Comments : "Approved";
                            _context.CashAdvanceRequests.Update(CashAdvanceReq);
                            await _context.SaveChangesAsync();
                        }



                        //update the CashAdvance request table to reflect the rejection
                        if (bRejectMessage)
                        {
                            var CashAdvanceReq = _context.CashAdvanceRequests.Where(p => p.Id == CashAdvanceStatusTrackerDto.CashAdvanceRequestId).FirstOrDefault();
                            CashAdvanceReq.ApprovalStatusTypeId = CashAdvanceStatusTrackerDto.ApprovalStatusTypeId ?? 0;
                            CashAdvanceReq.ApproverActionDate = DateTime.UtcNow;
                            CashAdvanceReq.Comments = bRejectMessage ? CashAdvanceStatusTrackerDto.Comments : "Approved";
                            _context.CashAdvanceRequests.Update(CashAdvanceReq);

                            //update the EmpCashAdvanceBalance to credit back the deducted amount
                            var empCashAdvanceBal = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == CashAdvanceReq.EmployeeId).FirstOrDefault();
                            empCashAdvanceBal.CurBalance = empCashAdvanceBal.CurBalance + CashAdvanceReq.CashAdvanceAmount;
                            empCashAdvanceBal.UpdatedOn = DateTime.UtcNow;
                            _context.EmpCurrentCashAdvanceBalances.Update(empCashAdvanceBal);


                            var disbursementsAndClaimsMaster = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == CashAdvanceReq.Id).FirstOrDefault();
                            disbursementsAndClaimsMaster.ApprovalStatusId = (int)EApprovalStatus.Rejected;
                            disbursementsAndClaimsMaster.ClaimAmount = CashAdvanceReq.CashAdvanceAmount;
                            disbursementsAndClaimsMaster.AmountToWallet = 0;
                            disbursementsAndClaimsMaster.AmountToCredit = bRejectMessage ? 0 : CashAdvanceReq.CashAdvanceAmount;
                            disbursementsAndClaimsMaster.IsSettledAmountCredited = false;
                            _context.DisbursementsAndClaimsMasters.Update(disbursementsAndClaimsMaster);
                            await _context.SaveChangesAsync();
                        }

                    }

                    //Project based Cash Advance request
                    else
                    {
                        //final approver hence update CashAdvanceRequest
                        claimitem = _context.CashAdvanceStatusTrackers.Where(c => c.CashAdvanceRequestId == CashAdvanceStatusTracker.CashAdvanceRequestId &&
                                    c.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).FirstOrDefault();
                        CashAdvanceStatusTracker.ApprovalStatusTypeId = CashAdvanceStatusTrackerDto.ApprovalStatusTypeId;



                        //Update CashAdvancerequest table to update the record to Approved as the final approver has approved it.
                        var CashAdvanceReq = await _context.CashAdvanceRequests.FindAsync(claimitem.CashAdvanceRequestId);
                        int? CashAdvanceReqId = CashAdvanceReq.Id;

                        //update the EmpCashAdvanceBalance to credit back the deducted amount
                        if (bRejectMessage)
                        {
                            var empCashAdvanceBal = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == CashAdvanceReq.EmployeeId).FirstOrDefault();
                            empCashAdvanceBal.CurBalance = empCashAdvanceBal.CurBalance + CashAdvanceReq.CashAdvanceAmount;
                            empCashAdvanceBal.UpdatedOn = DateTime.UtcNow;
                            _context.EmpCurrentCashAdvanceBalances.Update(empCashAdvanceBal);
                        }

                        CashAdvanceReq.ApprovalStatusTypeId = bRejectMessage ? (int)EApprovalStatus.Rejected : (int)EApprovalStatus.Approved;
                        CashAdvanceReq.ApproverActionDate = DateTime.UtcNow;


                        CashAdvanceReq.Comments = bRejectMessage ? CashAdvanceStatusTrackerDto.Comments : "Approved";
                        _context.Update(CashAdvanceReq);


                        //DisbursementAndClaimsMaster update the record to Approved (ApprovalStatusId
                        int? disbAndClaimItemId = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == claimitem.CashAdvanceRequestId).FirstOrDefault().Id;
                        var disbAndClaimItem = await _context.DisbursementsAndClaimsMasters.FindAsync(disbAndClaimItemId);

                        disbAndClaimItem.ApprovalStatusId = bRejectMessage ? (int)EApprovalStatus.Rejected : (int)EApprovalStatus.Approved;
                        disbAndClaimItem.ClaimAmount = CashAdvanceReq.CashAdvanceAmount;
                        disbAndClaimItem.AmountToWallet = 0;
                        disbAndClaimItem.AmountToCredit = bRejectMessage ? 0 : CashAdvanceReq.CashAdvanceAmount;
                        disbAndClaimItem.IsSettledAmountCredited = false;
                        _context.DisbursementsAndClaimsMasters.Update(disbAndClaimItem);
                        _context.Update(disbAndClaimItem);

                    }

                    _context.CashAdvanceStatusTrackers.Update(CashAdvanceStatusTracker);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                await AtoCashDbContextTransaction.CommitAsync();
            }
            RespStatus respStatus = new();

                if (bRejectMessage)
                {
                    respStatus.Status = "Success";
                    respStatus.Message = "Cash Advance(s) Rejected!";
                }
                else
                {
                    respStatus.Status = "Success";
                    respStatus.Message = "Cash Advance(s) Approved!";
                }
             
            return Ok(respStatus);
        }



        // POST: api/CashAdvanceStatusTrackers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, Manager, User")]
        public async Task<ActionResult<CashAdvanceStatusTracker>> PostCashAdvanceStatusTracker(CashAdvanceStatusTrackerDTO CashAdvanceStatusTrackerDto)
        {
            CashAdvanceStatusTracker CashAdvanceStatusTracker = new()
            {
                Id = CashAdvanceStatusTrackerDto.Id,
                EmployeeId = CashAdvanceStatusTrackerDto.EmployeeId,
                CashAdvanceRequestId = CashAdvanceStatusTrackerDto.CashAdvanceRequestId,
                BusinessUnitId = CashAdvanceStatusTrackerDto.BusinessUnitId,
                ProjectId = CashAdvanceStatusTrackerDto.ProjectId,
                JobRoleId = CashAdvanceStatusTrackerDto.JobRoleId,
                ApprovalLevelId = CashAdvanceStatusTrackerDto.ApprovalLevelId,
                RequestDate = CashAdvanceStatusTrackerDto.RequestDate,
                ApproverActionDate = CashAdvanceStatusTrackerDto.ApproverActionDate,
                ApprovalStatusTypeId = (int)EApprovalStatus.Pending,
                Comments = CashAdvanceStatusTrackerDto.Comments
            };

            _context.CashAdvanceStatusTrackers.Add(CashAdvanceStatusTracker);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCashAdvanceStatusTracker", new { id = CashAdvanceStatusTracker.Id }, CashAdvanceStatusTracker);
        }

        // DELETE: api/CashAdvanceStatusTrackers/5
        [HttpDelete("{id}")]
        //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> DeleteCashAdvanceStatusTracker(int id)
        {
            var CashAdvanceStatusTracker = await _context.CashAdvanceStatusTrackers.FindAsync(id);
            if (CashAdvanceStatusTracker == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "CashAdvance Approval Request Id invalid!" });
            }

            _context.CashAdvanceStatusTrackers.Remove(CashAdvanceStatusTracker);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Claim Deleted!" });
        }


        /// <summary>
        /// List of Pending approvals for the given Approver
        /// </summary>
        /// <param EmployeeId="id"></param>
        /// <returns>List of Claim</returns>

        [HttpGet("{id}")]
        [ActionName("ApprovalsPendingForApprover")]
        public ActionResult<IEnumerable<CashAdvanceStatusTrackerDTO>> ApprovalsPendingForApprover(int id)
        {

            if (id == 0)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id is Invalid" });
            }

            //get the RoleID of the Employee (Approver)
            Employee? apprEmp = _context.Employees.Find(id);

            if (apprEmp == null)
            {
                _logger.LogError("Employee Id is Invalid:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id is Invalid" });
            }

            List<EmployeeExtendedInfo>? listEmpExtendedInfo = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == id).ToList();


            List<CashAdvanceStatusTracker> ListCashAdvanceStatusTrackers = new();

            //for Non-Project approval status trackers
            foreach (var empExtInfo in listEmpExtendedInfo)
            {
                ListCashAdvanceStatusTrackers.AddRange(_context.CashAdvanceStatusTrackers.Where(r => r.JobRoleId == empExtInfo.JobRoleId
                                                                                                         && r.ApprovalGroupId == empExtInfo.ApprovalGroupId
                                                                                                         && r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending
                                                                                                         && r.ProjManagerId == null).ToList());
            }
            //for Project based approval status trackers
            ListCashAdvanceStatusTrackers.AddRange(_context.CashAdvanceStatusTrackers.Where(r => r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending
                                                                                                         && r.ProjManagerId == id).ToList());

            List<CashAdvanceStatusTrackerDTO> ListCashAdvanceStatusTrackerDTO = new();

            foreach (CashAdvanceStatusTracker CashAdvanceStatusTracker in ListCashAdvanceStatusTrackers)
            {

                var CashAdvanceReq = _context.CashAdvanceRequests.Find(CashAdvanceStatusTracker.CashAdvanceRequestId);
                CashAdvanceStatusTrackerDTO CashAdvanceStatusTrackerDTO = new()
                {
                    Id = CashAdvanceStatusTracker.Id,
                    EmployeeId = CashAdvanceStatusTracker.EmployeeId,
                    EmployeeName = _context.Employees.Find(CashAdvanceStatusTracker.EmployeeId).GetFullName(),
                    BusinessTypeId = CashAdvanceStatusTracker.BusinessTypeId,
                    BusinessType = CashAdvanceStatusTracker.BusinessTypeId != null ? _context.BusinessTypes.Find(CashAdvanceStatusTracker.BusinessTypeId).BusinessTypeName : null,
                    CashAdvanceRequestId = CashAdvanceStatusTracker.CashAdvanceRequestId,
                    BusinessUnitId = CashAdvanceStatusTracker.BusinessUnitId,
                    BusinessUnit = CashAdvanceStatusTracker.BusinessUnitId != null ? _context.BusinessUnits.Find(CashAdvanceStatusTracker.BusinessUnitId).GetBusinessUnitName() : null,
                    ProjectId = CashAdvanceStatusTracker.ProjectId,
                    ProjectName = CashAdvanceStatusTracker.ProjectId != null ? _context.Projects.Find(CashAdvanceStatusTracker.ProjectId).ProjectName : null,
                    JobRoleId = CashAdvanceStatusTracker.JobRoleId,
                    JobRole = CashAdvanceStatusTracker.JobRoleId != null ? _context.JobRoles.Find(CashAdvanceStatusTracker.JobRoleId).GetJobRole() : null,
                    ApprovalLevelId = CashAdvanceStatusTracker.ApprovalLevelId,
                    ClaimAmount = CashAdvanceReq.CashAdvanceAmount,
                    RequestDate = CashAdvanceStatusTracker.RequestDate,
                    ApproverActionDate = CashAdvanceStatusTracker.ApproverActionDate,
                    ApprovalStatusTypeId = CashAdvanceStatusTracker.ApprovalStatusTypeId,
                    ApprovalStatusType = _context.ApprovalStatusTypes.Find(CashAdvanceStatusTracker.ApprovalStatusTypeId).Status,
                    Comments = CashAdvanceStatusTracker.Comments
                };


                ListCashAdvanceStatusTrackerDTO.Add(CashAdvanceStatusTrackerDTO);

            }


            return Ok(ListCashAdvanceStatusTrackerDTO.OrderByDescending(o => o.RequestDate).ToList());

        }



        //To get the counts of pending approvals

        [HttpGet("{id}")]
        [ActionName("CountOfApprovalsPendingForApprover")]
        public ActionResult<int> GetCountOfApprovalsPendingForApprover(int id)
        {

            if (id == 0)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id is Invalid" });
            }

            List<EmployeeExtendedInfo>? ListEmpExtInfo = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == id).ToList();


            List<CashAdvanceStatusTracker> ListCashAdvanceStatusTrackers = new();

            //for Non-Project approval status trackers
            foreach (var empExtInfo in ListEmpExtInfo)
            {
                ListCashAdvanceStatusTrackers.AddRange(_context.CashAdvanceStatusTrackers.Where(r => r.JobRoleId == empExtInfo.JobRoleId
                                                                                                         && r.ApprovalGroupId == empExtInfo.ApprovalGroupId
                                                                                                         && r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending
                                                                                                         && r.ProjManagerId == null).ToList());
            }
            //for Project based approval status trackers
            ListCashAdvanceStatusTrackers.AddRange(_context.CashAdvanceStatusTrackers.Where(r => r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending
                                                                                                         && r.ProjManagerId == null).ToList());

            List<CashAdvanceStatusTrackerDTO> ListCashAdvanceStatusTrackerDTO = new();

            foreach (CashAdvanceStatusTracker CashAdvanceStatusTracker in ListCashAdvanceStatusTrackers)
            {

                var CashAdvanceReq = _context.CashAdvanceRequests.Find(CashAdvanceStatusTracker.CashAdvanceRequestId);
                CashAdvanceStatusTrackerDTO CashAdvanceStatusTrackerDTO = new()
                {
                    Id = CashAdvanceStatusTracker.Id,
                    EmployeeId = CashAdvanceStatusTracker.EmployeeId,
                    EmployeeName = _context.Employees.Find(CashAdvanceStatusTracker.EmployeeId).GetFullName(),
                    BusinessTypeId = CashAdvanceStatusTracker.BusinessTypeId,
                    BusinessType = CashAdvanceStatusTracker.BusinessTypeId != null ? _context.BusinessTypes.Find(CashAdvanceStatusTracker.BusinessTypeId).BusinessTypeName : null,
                    CashAdvanceRequestId = CashAdvanceStatusTracker.CashAdvanceRequestId,
                    BusinessUnitId = CashAdvanceStatusTracker.BusinessUnitId,
                    BusinessUnit = CashAdvanceStatusTracker.BusinessUnitId != null ? _context.BusinessUnits.Find(CashAdvanceStatusTracker.BusinessUnitId).GetBusinessUnitName() : null,
                    ProjectId = CashAdvanceStatusTracker.ProjectId,
                    ProjectName = CashAdvanceStatusTracker.ProjectId != null ? _context.Projects.Find(CashAdvanceStatusTracker.ProjectId).ProjectName : null,
                    JobRoleId = CashAdvanceStatusTracker.JobRoleId,
                    JobRole = _context.JobRoles.Find(CashAdvanceStatusTracker.JobRoleId).GetJobRole(),
                    ApprovalLevelId = CashAdvanceStatusTracker.ApprovalLevelId,
                    ClaimAmount = CashAdvanceReq.CashAdvanceAmount,
                    RequestDate = CashAdvanceStatusTracker.RequestDate,
                    ApproverActionDate = CashAdvanceStatusTracker.ApproverActionDate,
                    ApprovalStatusTypeId = CashAdvanceStatusTracker.ApprovalStatusTypeId,
                    ApprovalStatusType = _context.ApprovalStatusTypes.Find(CashAdvanceStatusTracker.ApprovalStatusTypeId).Status,
                    Comments = CashAdvanceStatusTracker.Comments
                };


                ListCashAdvanceStatusTrackerDTO.Add(CashAdvanceStatusTrackerDTO);

            }

            return 0;
        }

        /// <summary>
        /// GetApprovalFlowForRequest
        /// </summary>
        /// <param CashAdvanceRequestId="id"></param>
        /// <returns></returns>

        [HttpGet("{id}")]
        [ActionName("ApprovalFlowForRequest")]
        public ActionResult<IEnumerable<ApprovalStatusFlowVM>> GetApprovalFlowForRequest(int id)
        {

            if (id == 0)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "CashAdvanceRequest Id is Invalid" });
            }



            var claimRequestTracks = _context.CashAdvanceStatusTrackers.Where(c => c.CashAdvanceRequestId == id).OrderBy(x => x.JobRoleId).ToList();

            List<ApprovalStatusFlowVM> ListApprovalStatusFlow = new();

            if (claimRequestTracks == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "CashAdvanceRequest Id is Not Found" });
            }
            else
            {
                int reqEmpId = claimRequestTracks[0].EmployeeId ?? 0;
                Employee reqEmp = _context.Employees.Find(reqEmpId);

                //add requester to the approval flow with Level 0
                ApprovalStatusFlowVM requesterInApprovalFlow = new();
                requesterInApprovalFlow.ApprovalLevel = 0;
                requesterInApprovalFlow.ApproverRole = "Requestor";
                requesterInApprovalFlow.ApproverName = reqEmp.GetFullName();
                requesterInApprovalFlow.ApproverActionDate = claimRequestTracks[0].RequestDate;
                requesterInApprovalFlow.ApprovalStatusType = _context.ApprovalStatusTypes.Find((int)EApprovalStatus.Intitated).Status;


                ListApprovalStatusFlow.Add(requesterInApprovalFlow);
            }


            foreach (CashAdvanceStatusTracker claim in claimRequestTracks)
            {
                string claimApproverName = null;

                if (claim.ProjectId > 0)
                {
                    claimApproverName = _context.Employees.Where(e => e.Id == _context.Projects.Find(claim.ProjectId).ProjectManagerId)
                        .Select(s => s.GetFullName()).FirstOrDefault();
                }
                else
                {
                    EmployeeExtendedInfo apprEmpExtInfo = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == claim.JobRoleId && e.BusinessUnitId == claim.BusinessUnitId).FirstOrDefault();

                    claimApproverName = _context.Employees.Find(apprEmpExtInfo.EmployeeId).GetFullName();

                }



                ApprovalStatusFlowVM approvalStatusFlow = new();
                approvalStatusFlow.ApprovalLevel = claim.ApprovalLevelId;
                approvalStatusFlow.ApproverRole = claim.ProjectId == null ? _context.JobRoles.Find(claim.JobRoleId).GetJobRole() : "Project Manager";
                approvalStatusFlow.ApproverName = claimApproverName;
                approvalStatusFlow.ApproverActionDate = claim.ApproverActionDate;
                approvalStatusFlow.ApprovalStatusType = _context.ApprovalStatusTypes.Find(claim.ApprovalStatusTypeId).Status;


                ListApprovalStatusFlow.Add(approvalStatusFlow);
            }

            return Ok(ListApprovalStatusFlow);

        }




        ////
    }
}
