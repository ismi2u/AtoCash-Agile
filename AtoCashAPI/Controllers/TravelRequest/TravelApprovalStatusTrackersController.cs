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
using EmailService;
using AtoCashAPI.Authentication;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace AtoCashAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User, Manager")]
    public class TravelApprovalStatusTrackersController : ControllerBase
    {
        private readonly AtoCashDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<TravelApprovalStatusTrackersController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;


        public TravelApprovalStatusTrackersController(AtoCashDbContext context, IEmailSender emailSender,
                                                        ILogger<TravelApprovalStatusTrackersController> logger,
                                                         UserManager<ApplicationUser> userManager,
                                                         IConfiguration config)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
            _config = config;
        }




        // GET: api/TravelApprovalStatusTrackers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TravelApprovalStatusTrackerDTO>>> GetTravelApprovalStatusTrackers()
        {
            List<TravelApprovalStatusTrackerDTO> ListTravelApprovalStatusTrackerDTO = new();



            var TravelApprovalStatusTrackers = await _context.TravelApprovalStatusTrackers.ToListAsync();

            foreach (TravelApprovalStatusTracker travelApprovalStatusTracker in TravelApprovalStatusTrackers)
            {


                TravelApprovalStatusTrackerDTO travelApprovalStatusTrackerDTO = new();

                travelApprovalStatusTrackerDTO.Id = travelApprovalStatusTracker.Id;
                travelApprovalStatusTrackerDTO.EmployeeId = travelApprovalStatusTracker.EmployeeId;
                travelApprovalStatusTrackerDTO.TravelStartDate = travelApprovalStatusTracker.TravelStartDate;
                travelApprovalStatusTrackerDTO.TravelEndDate = travelApprovalStatusTracker.TravelEndDate;
                travelApprovalStatusTrackerDTO.EmployeeName = _context.Employees.Find(travelApprovalStatusTracker.EmployeeId).GetFullName();
                travelApprovalStatusTrackerDTO.TravelApprovalRequestId = travelApprovalStatusTracker.TravelApprovalRequestId;

                travelApprovalStatusTrackerDTO.BusinessTypeId = travelApprovalStatusTracker.BusinessTypeId;
                travelApprovalStatusTrackerDTO.BusinessType = travelApprovalStatusTracker.BusinessTypeId != null ? _context.BusinessTypes.Find(travelApprovalStatusTracker.BusinessTypeId).BusinessTypeName : null;
                travelApprovalStatusTrackerDTO.BusinessUnitId = travelApprovalStatusTracker.BusinessUnitId;
                travelApprovalStatusTrackerDTO.BusinessUnit = travelApprovalStatusTracker.BusinessUnitId != null ? _context.BusinessUnits.Find(travelApprovalStatusTracker.BusinessUnitId).GetBusinessUnitName() : null;

                travelApprovalStatusTrackerDTO.ProjectId = travelApprovalStatusTracker.ProjectId;
                travelApprovalStatusTrackerDTO.ProjectName = travelApprovalStatusTracker.ProjectId != null ? _context.Projects.Find(travelApprovalStatusTracker.ProjectId).ProjectName : null;
                travelApprovalStatusTrackerDTO.JobRoleId = travelApprovalStatusTracker.JobRoleId;
                travelApprovalStatusTrackerDTO.JobRole = _context.JobRoles.Find(travelApprovalStatusTracker.JobRoleId).GetJobRole();
                travelApprovalStatusTrackerDTO.ApprovalLevelId = travelApprovalStatusTracker.ApprovalLevelId;
                travelApprovalStatusTrackerDTO.RequestDate = travelApprovalStatusTracker.RequestDate;
                travelApprovalStatusTrackerDTO.ApproverActionDate = travelApprovalStatusTracker.ApproverActionDate;
                travelApprovalStatusTrackerDTO.ApprovalStatusTypeId = travelApprovalStatusTracker.ApprovalStatusTypeId;
                travelApprovalStatusTrackerDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(travelApprovalStatusTracker.ApprovalStatusTypeId).Status;
                travelApprovalStatusTrackerDTO.Comments = travelApprovalStatusTracker.Comments;


                ListTravelApprovalStatusTrackerDTO.Add(travelApprovalStatusTrackerDTO);

            }

            return ListTravelApprovalStatusTrackerDTO.OrderByDescending(o => o.RequestDate).ToList();
        }



        // GET: api/TravelApprovalStatusTrackers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TravelApprovalStatusTrackerDTO>> GetTravelApprovalStatusTracker(int id)
        {

            TravelApprovalStatusTrackerDTO travelApprovalStatusTrackerDTO = new();

            var travelApprovalStatusTracker = _context.TravelApprovalStatusTrackers.Where(t => t.TravelApprovalRequestId == id).FirstOrDefault();

            if (travelApprovalStatusTracker == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Invalid Travel Approval Status Id!" });
            }

            await Task.Run(() =>
            {
                travelApprovalStatusTrackerDTO.Id = travelApprovalStatusTracker.Id;
                travelApprovalStatusTrackerDTO.EmployeeId = travelApprovalStatusTracker.EmployeeId;
                travelApprovalStatusTrackerDTO.TravelStartDate = travelApprovalStatusTracker.TravelStartDate;
                travelApprovalStatusTrackerDTO.TravelEndDate = travelApprovalStatusTracker.TravelEndDate;
                travelApprovalStatusTrackerDTO.EmployeeName = _context.Employees.Find(travelApprovalStatusTracker.EmployeeId).GetFullName();
                travelApprovalStatusTrackerDTO.TravelApprovalRequestId = travelApprovalStatusTracker.TravelApprovalRequestId;

                travelApprovalStatusTrackerDTO.BusinessTypeId = travelApprovalStatusTracker.BusinessTypeId;
                travelApprovalStatusTrackerDTO.BusinessType = travelApprovalStatusTracker.BusinessTypeId != null ? _context.BusinessTypes.Find(travelApprovalStatusTracker.BusinessTypeId).BusinessTypeName : null;
                travelApprovalStatusTrackerDTO.BusinessUnitId = travelApprovalStatusTracker.BusinessUnitId;
                travelApprovalStatusTrackerDTO.BusinessUnit = travelApprovalStatusTracker.BusinessUnitId != null ? _context.BusinessUnits.Find(travelApprovalStatusTracker.BusinessUnitId).GetBusinessUnitName() : null;

                travelApprovalStatusTrackerDTO.ProjectId = travelApprovalStatusTracker.ProjectId;
                travelApprovalStatusTrackerDTO.ProjectName = travelApprovalStatusTracker.ProjectId != null ? _context.Projects.Find(travelApprovalStatusTracker.ProjectId).ProjectName : null;
                travelApprovalStatusTrackerDTO.JobRoleId = travelApprovalStatusTracker.JobRoleId;
                travelApprovalStatusTrackerDTO.JobRole = _context.JobRoles.Find(travelApprovalStatusTracker.JobRoleId).GetJobRole();
                travelApprovalStatusTrackerDTO.ApprovalLevelId = travelApprovalStatusTracker.ApprovalLevelId;
                travelApprovalStatusTrackerDTO.RequestDate = travelApprovalStatusTracker.RequestDate;
                travelApprovalStatusTrackerDTO.ApproverActionDate = travelApprovalStatusTracker.ApproverActionDate;
                travelApprovalStatusTrackerDTO.ApprovalStatusTypeId = travelApprovalStatusTracker.ApprovalStatusTypeId;
                travelApprovalStatusTrackerDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(travelApprovalStatusTracker.ApprovalStatusTypeId).Status;
                travelApprovalStatusTrackerDTO.Comments = travelApprovalStatusTracker.Comments;
            });


            return Ok(travelApprovalStatusTrackerDTO);
        }

        // PUT: api/TravelApprovalStatusTrackers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPut]
        //  [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, Manager")]
        public async Task<IActionResult> PutTravelApprovalStatusTracker(List<TravelApprovalStatusTrackerDTO> ListTravelApprovalStatusTrackerDTO)
        {

            if (ListTravelApprovalStatusTrackerDTO.Count == 0)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "No Request to Approve!" });
            }


            bool isNextApproverAvailable = true;
            bool bRejectMessage = false;
            //ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {
                foreach (TravelApprovalStatusTrackerDTO travelApprovalStatusTrackerDTO in ListTravelApprovalStatusTrackerDTO)
                {
                    var travelApprovalStatusTracker = await _context.TravelApprovalStatusTrackers.FindAsync(travelApprovalStatusTrackerDTO.Id);

                    //if same status continue to next loop, otherwise process
                    if (travelApprovalStatusTracker.ApprovalStatusTypeId == travelApprovalStatusTrackerDTO.ApprovalStatusTypeId)
                    {
                        continue;
                    }
                    if (travelApprovalStatusTrackerDTO.ApprovalStatusTypeId == (int)EApprovalStatus.Rejected)
                    {
                        bRejectMessage = true;
                    }


                    travelApprovalStatusTracker.Id = travelApprovalStatusTrackerDTO.Id;
                    travelApprovalStatusTracker.EmployeeId = travelApprovalStatusTrackerDTO.EmployeeId;

                    travelApprovalStatusTracker.TravelApprovalRequestId = travelApprovalStatusTrackerDTO.TravelApprovalRequestId ?? 0;

                    travelApprovalStatusTracker.BusinessTypeId = travelApprovalStatusTrackerDTO.BusinessTypeId;
                    travelApprovalStatusTracker.BusinessUnitId = travelApprovalStatusTrackerDTO.BusinessUnitId;


                    travelApprovalStatusTracker.ProjectId = travelApprovalStatusTrackerDTO.ProjectId;
                    travelApprovalStatusTracker.JobRoleId = travelApprovalStatusTrackerDTO.JobRoleId;
                    travelApprovalStatusTracker.ApprovalLevelId = travelApprovalStatusTrackerDTO.ApprovalLevelId;
                    travelApprovalStatusTracker.RequestDate = travelApprovalStatusTrackerDTO.RequestDate;
                    travelApprovalStatusTracker.ApproverActionDate = DateTime.UtcNow;
                    travelApprovalStatusTracker.ApproverEmpId = int.Parse( HttpContext.User.Claims.First(c => c.Type == "EmployeeId").Value);
                    travelApprovalStatusTracker.Comments = bRejectMessage ? travelApprovalStatusTrackerDTO.Comments : "Approved";




                    TravelApprovalStatusTracker travelItem;

                    if (travelApprovalStatusTrackerDTO.BusinessUnitId != null)
                    {
                        var employee = await _context.Employees.FindAsync(travelApprovalStatusTracker.EmployeeId);
                        var ListEmpExtInfo = await _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == employee.Id).ToListAsync();


                        //Check if the record is already approved
                        //if it is not approved then trigger next approver level email & Change the status to approved
                        if (travelApprovalStatusTrackerDTO.ApprovalStatusTypeId == (int)EApprovalStatus.Approved)
                        {
                            //Get the next approval level (get its ID)
                            int qTravelApprovalRequestId = travelApprovalStatusTrackerDTO.TravelApprovalRequestId ?? 0;

                            isNextApproverAvailable = true;

                            int curTravelApprovalLevel = _context.ApprovalLevels.Find(travelApprovalStatusTrackerDTO.ApprovalLevelId).Level;
                            int nextTravelApprovalLevel = curTravelApprovalLevel + 1;
                            int qApprovalLevelId;
                            int? apprGroupId = travelApprovalStatusTracker.ApprovalGroupId;


                            if (_context.ApprovalRoleMaps.Where(a => a.ApprovalGroupId == apprGroupId && a.ApprovalLevelId == nextTravelApprovalLevel).FirstOrDefault() != null)
                            {
                                qApprovalLevelId = _context.ApprovalLevels.Where(x => x.Level == nextTravelApprovalLevel).FirstOrDefault().Id;
                            }
                            else
                            {
                                qApprovalLevelId = _context.ApprovalLevels.Where(x => x.Level == curTravelApprovalLevel).FirstOrDefault().Id;
                                isNextApproverAvailable = false;
                            }

                            int qApprovalStatusTypeId = isNextApproverAvailable ? (int)EApprovalStatus.Intitated : (int)EApprovalStatus.Pending;

                            //update the next level approver Track request to PENDING (from Initiating) 
                            //if claimitem is not null change the status
                            if (isNextApproverAvailable)
                            {


                                travelItem = _context.TravelApprovalStatusTrackers
                                                                   .Where(c =>
                                                                   c.TravelApprovalRequestId == qTravelApprovalRequestId &&
                                                                   c.ApprovalStatusTypeId == qApprovalStatusTypeId &&
                                                                   c.ApprovalGroupId == apprGroupId &&
                                                                   c.ApprovalLevelId == qApprovalLevelId).FirstOrDefault();

                                if (travelItem != null)
                                {
                                    travelItem.ApprovalStatusTypeId = (int)EApprovalStatus.Pending;
                                }

                            }
                            else
                            {
                                //final approver hence update TravelApprovalStatusTracker
                                travelItem = _context.TravelApprovalStatusTrackers.Where(c => c.TravelApprovalRequestId == qTravelApprovalRequestId &&
                               c.ApprovalStatusTypeId == qApprovalStatusTypeId &&
                               c.ApprovalGroupId == apprGroupId &&
                               c.ApprovalLevelId == qApprovalLevelId).FirstOrDefault();
                                //claimitem.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                                travelItem.ApproverActionDate = DateTime.UtcNow;
                                travelItem.ApproverEmpId = int.Parse( HttpContext.User.Claims.First(c => c.Type == "EmployeeId").Value);


                                //final Approver hence updating TravelApprovalRequest
                                var travelApprovalRequest = _context.TravelApprovalRequests.Find(qTravelApprovalRequestId);
                                travelApprovalRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                                travelApprovalRequest.Comments = bRejectMessage ? travelApprovalStatusTrackerDTO.Comments : "Approved";
                                travelApprovalRequest.ApproverActionDate = DateTime.UtcNow;
                                _context.Update(travelApprovalRequest);

                            }

                            //Save to database
                            if (travelItem != null) { _context.Update(travelItem); };
                            await _context.SaveChangesAsync();

                            //Save to database
                            var getEmpClaimApproversAllLevels = _context.ApprovalRoleMaps.Include(a => a.ApprovalLevel).Where(a => a.ApprovalGroupId == apprGroupId).OrderBy(o => o.ApprovalLevel.Level).ToList();

                            foreach (var ApprMap in getEmpClaimApproversAllLevels)
                            {

                                //only next level (level + 1) approver is considered here
                                if (ApprMap.ApprovalLevelId == travelApprovalStatusTracker.ApprovalLevelId + 1)
                                {
                                    int? jobrole_id = ApprMap.JobRoleId;

                                    var apprEmpId = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == jobrole_id && e.ApprovalGroupId == apprGroupId).FirstOrDefault().EmployeeId;

                                    var approver = await _context.Employees.FindAsync(apprEmpId);

                                    //##### 4. Send email to the Approver
                                    //####################################
                                    string[] paths = { Directory.GetCurrentDirectory(), "TravelApprNotificationEmail.html" };
                                    string FilePath = Path.Combine(paths);
                                    StreamReader str = new StreamReader(FilePath);
                                    _logger.LogInformation("Email template path " + FilePath);
                                    string MailText = str.ReadToEnd();
                                    str.Close();

                                    var approverMailAddress = approver.Email;
                                    string subject = "Travel Approval Request " + travelApprovalStatusTracker.TravelApprovalRequestId.ToString();
                                    Employee emp = await _context.Employees.FindAsync(travelApprovalStatusTracker.EmployeeId);
                                    var travelreq = _context.TravelApprovalRequests.Find(travelApprovalStatusTracker.TravelApprovalRequestId);
                                    //var travelreqStatusTracker = _context.TravelApprovalStatusTrackers.Find(travelApprovalStatusTracker.Id);
                                    
                                    var domain = _config.GetSection("FrontendDomain").Value;
                                    MailText = MailText.Replace("{FrontendDomain}", domain);

                                    var builder = new MimeKit.BodyBuilder();

                                    MailText = MailText.Replace("{Requester}", emp.GetFullName());
                                    MailText = MailText.Replace("{ApproverName}", approver.GetFullName()); //
                                    MailText = MailText.Replace("{Request}", travelreq.TravelStartDate.Value.ToShortDateString() + " - " + travelreq.TravelEndDate.Value.ToShortDateString() + " (Purpose): " + travelreq.TravelPurpose.ToString());
                                    MailText = MailText.Replace("{RequestNumber}", travelreq.Id.ToString());
                                    builder.HtmlBody = MailText;

                                    EmailDto emailDto = new EmailDto();
                                    emailDto.To = approverMailAddress;
                                    emailDto.Subject = subject;
                                    emailDto.Body = builder.HtmlBody;

                                    await _emailSender.SendEmailAsync(emailDto);

                                    break;
                                    _logger.LogInformation("Travel Request modified/Updated : approver " + approver.GetFullName() + "Email Sent");

                                }
                            }
                        }

                        //if nothing else then just update the approval status
                        //if nothing else then just update the approval status
                        travelApprovalStatusTracker.ApprovalStatusTypeId = travelApprovalStatusTrackerDTO.ApprovalStatusTypeId;

                        //If no travelApprovalStatusTrackers are in pending for the travel request then update the TravelRequest table

                        int pendingApprovals = _context.TravelApprovalStatusTrackers
                                  .Where(t => t.TravelApprovalRequestId == travelApprovalStatusTrackerDTO.TravelApprovalRequestId &&
                                  t.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).Count();

                        if (pendingApprovals == 0)
                        {
                            var trvlApprReq = _context.TravelApprovalRequests.Where(p => p.Id == travelApprovalStatusTrackerDTO.TravelApprovalRequestId).FirstOrDefault();
                            trvlApprReq.ApprovalStatusTypeId = travelApprovalStatusTrackerDTO.ApprovalStatusTypeId;
                            trvlApprReq.ApproverActionDate = DateTime.UtcNow;
                            trvlApprReq.Comments = travelApprovalStatusTrackerDTO.Comments;
                            _context.TravelApprovalRequests.Update(trvlApprReq);
                            await _context.SaveChangesAsync();
                        }

                        //update the Travel request table to reflect the rejection
                        if (bRejectMessage)
                        {
                            var trvlApprReq = _context.TravelApprovalRequests.Where(p => p.Id == travelApprovalStatusTrackerDTO.TravelApprovalRequestId).FirstOrDefault();
                            trvlApprReq.ApprovalStatusTypeId = travelApprovalStatusTrackerDTO.ApprovalStatusTypeId;
                            trvlApprReq.ApproverActionDate = DateTime.UtcNow;
                            trvlApprReq.Comments = travelApprovalStatusTrackerDTO.Comments;
                            _context.TravelApprovalRequests.Update(trvlApprReq);
                            await _context.SaveChangesAsync();
                        }

                    }

                    //Project based Cash Advance request
                    else
                    {
                        //final approver hence update TravelApprovalRequest

                        travelItem = _context.TravelApprovalStatusTrackers.Where(c => c.TravelApprovalRequestId == travelApprovalStatusTracker.TravelApprovalRequestId &&
                                    c.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).FirstOrDefault();

                        travelApprovalStatusTracker.ApprovalStatusTypeId = travelApprovalStatusTrackerDTO.ApprovalStatusTypeId;



                        //Update TravelApprovalRequests table to update the record to Approved as the final approver has approved it.
                        var travelApprovalrequest = await _context.TravelApprovalRequests.FindAsync(travelItem.TravelApprovalRequestId);
                        int? travelApprovalrequestId = travelApprovalrequest.Id;

                        //Dont  update the EmpCashAdvanceBalance to credit back the deducted amount as this is Travel request
                        //if (bRejectMessage)
                        //{
                        //    var empCashAdvanceBal = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == CashAdvanceReq.EmployeeId).FirstOrDefault();
                        //    empCashAdvanceBal.CurBalance = empCashAdvanceBal.CurBalance + CashAdvanceReq.CashAdvanceAmount;
                        //    empCashAdvanceBal.UpdatedOn = DateTime.UtcNow;
                        //    _context.EmpCurrentCashAdvanceBalances.Update(empCashAdvanceBal);
                        //}

                        travelApprovalrequest.ApprovalStatusTypeId = bRejectMessage ? (int)EApprovalStatus.Rejected : (int)EApprovalStatus.Approved;
                        travelApprovalrequest.ApproverActionDate = DateTime.UtcNow;

                        travelApprovalrequest.Comments = bRejectMessage ? travelApprovalStatusTrackerDTO.Comments : "Approved";
                        _context.Update(travelApprovalrequest);


                        var requester = await _context.Employees.FindAsync(travelApprovalrequest.EmployeeId);

                        _logger.LogInformation(requester.GetFullName() + " Travel Request Approved - Email Start");

                        string[] paths = { Directory.GetCurrentDirectory(), "TravelApproved.html" };
                        string FilePath = Path.Combine(paths);
                        _logger.LogInformation("Email template path " + FilePath);
                        StreamReader str = new StreamReader(FilePath);
                        string MailText = str.ReadToEnd();
                        str.Close();

                        var requestId = travelApprovalrequest.Id;

                        string subject = "Travel Request #" + requestId + "is approved " ;

                        var domain = _config.GetSection("FrontendDomain").Value;
                        MailText = MailText.Replace("{FrontendDomain}", domain);

                        var builder = new MimeKit.BodyBuilder();

                        MailText = MailText.Replace("{Requester}", requester.GetFullName());
                        MailText = MailText.Replace("{RequestNumber}", requestId.ToString());
                        builder.HtmlBody = MailText;

                        EmailDto emailDto = new EmailDto();
                        emailDto.To = requester.Email;
                        emailDto.Subject = subject;
                        emailDto.Body = builder.HtmlBody;

                        await _emailSender.SendEmailAsync(emailDto);
                        _logger.LogInformation(requester.GetFullName() + "Your Travel Request Approved - Email Sent");




                    }

                    _context.TravelApprovalStatusTrackers.Update(travelApprovalStatusTracker);
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
                respStatus.Message = "Travel Request(s) Rejected!";
            }
            else
            {
                respStatus.Status = "Success";
                respStatus.Message = "Travel Request(s) Approved!";
            }

            return Ok(respStatus);
        }


        [HttpGet("{id}")]
        [ActionName("ApprovalsPendingForApprover")]
        public ActionResult<IEnumerable<TravelApprovalStatusTrackerDTO>> ApprovalsPendingForApprover(int id)
        {
            if (id == 0)
            {
                _logger.LogError("ApprovalsPendingForApprover Employee Id is Invalid:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id is Invalid" });
            }

            //get the RoleID of the Employee (Approver)
            Employee? apprEmp = _context.Employees.Find(id);

            if (apprEmp == null)
            {
                _logger.LogError("Employee Id is Invalid:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id is Invalid" });
            }

            var listEmpExtendedInfo = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == id).ToList();


            List<TravelApprovalStatusTracker> ListTravelApprovalStatusTrackers = new();



            //for Non-Project approval status trackers
            foreach (var empExtInfo in listEmpExtendedInfo)
            {
                ListTravelApprovalStatusTrackers.AddRange(_context.TravelApprovalStatusTrackers.Where(r => r.JobRoleId == empExtInfo.JobRoleId
                                                                                                         && r.ApprovalGroupId == empExtInfo.ApprovalGroupId
                                                                                                         && r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending
                                                                                                         && r.ProjManagerId == null).ToList());
            }
            //for Project based approval status trackers
            ListTravelApprovalStatusTrackers.AddRange(_context.TravelApprovalStatusTrackers.Where(r => r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending
                                                                                                         && r.ProjManagerId == id).ToList());


            List<TravelApprovalStatusTrackerDTO> ListTravelApprovalStatusTrackerDTO = new();


            foreach (TravelApprovalStatusTracker travelApprovalStatusTracker in ListTravelApprovalStatusTrackers)
            {
                TravelApprovalStatusTrackerDTO travelApprovalStatusTrackerDTO = new();


                travelApprovalStatusTrackerDTO.Id = travelApprovalStatusTracker.Id;
                travelApprovalStatusTrackerDTO.EmployeeId = travelApprovalStatusTracker.EmployeeId;
                travelApprovalStatusTrackerDTO.TravelStartDate = travelApprovalStatusTracker.TravelStartDate;
                travelApprovalStatusTrackerDTO.TravelEndDate = travelApprovalStatusTracker.TravelEndDate;
                travelApprovalStatusTrackerDTO.EmployeeName = _context.Employees.Find(travelApprovalStatusTracker.EmployeeId).GetFullName();
                travelApprovalStatusTrackerDTO.TravelApprovalRequestId = travelApprovalStatusTracker.TravelApprovalRequestId;

                travelApprovalStatusTrackerDTO.BusinessTypeId = travelApprovalStatusTracker.BusinessTypeId;
                travelApprovalStatusTrackerDTO.BusinessType = travelApprovalStatusTracker.BusinessTypeId != null ? _context.BusinessTypes.Find(travelApprovalStatusTracker.BusinessTypeId).BusinessTypeName : null;
                travelApprovalStatusTrackerDTO.BusinessUnitId = travelApprovalStatusTracker.BusinessUnitId;
                travelApprovalStatusTrackerDTO.BusinessUnit = travelApprovalStatusTracker.BusinessUnitId != null ? _context.BusinessUnits.Find(travelApprovalStatusTracker.BusinessUnitId).GetBusinessUnitName() : null;

                travelApprovalStatusTrackerDTO.ProjectId = travelApprovalStatusTracker.ProjectId;
                travelApprovalStatusTrackerDTO.ProjectName = travelApprovalStatusTracker.ProjectId != null ? _context.Projects.Find(travelApprovalStatusTracker.ProjectId).ProjectName : null;
                travelApprovalStatusTrackerDTO.JobRoleId = travelApprovalStatusTracker.JobRoleId;
                travelApprovalStatusTrackerDTO.JobRole = travelApprovalStatusTracker.JobRoleId != null ? _context.JobRoles.Find(travelApprovalStatusTracker.JobRoleId).GetJobRole() : null;
                travelApprovalStatusTrackerDTO.ApprovalLevelId = travelApprovalStatusTracker.ApprovalLevelId;
                travelApprovalStatusTrackerDTO.RequestDate = travelApprovalStatusTracker.RequestDate;
                travelApprovalStatusTrackerDTO.ApproverActionDate = travelApprovalStatusTracker.ApproverActionDate;
                travelApprovalStatusTrackerDTO.ApprovalStatusTypeId = travelApprovalStatusTracker.ApprovalStatusTypeId;
                travelApprovalStatusTrackerDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(travelApprovalStatusTracker.ApprovalStatusTypeId).Status;
                travelApprovalStatusTrackerDTO.Comments = travelApprovalStatusTracker.Comments;



                ListTravelApprovalStatusTrackerDTO.Add(travelApprovalStatusTrackerDTO);

            }


            return Ok(ListTravelApprovalStatusTrackerDTO.OrderByDescending(o => o.RequestDate).ToList());
        }


        [HttpGet("{id}")]
        [ActionName("CountOfApprovalsPendingForApprover")]
        public ActionResult<int> GetCountOfApprovalsPendingForApprover(int id)
        {

            int CountOfApprovalsPending = 0;

            if (id == 0)
            {
                _logger.LogError("GetCountOfApprovalsPendingForApprover Employee id is null:" + id);
                return NotFound(new RespStatus { Status = "Failure", Message = "Employee Id is Invalid" });
            }

            var listEmpExtendedInfo = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == id).ToList();

            foreach (EmployeeExtendedInfo empExtInfo in listEmpExtendedInfo)
            {

                //get the RoleID of the Employee (Approver)
                int? jobroleid = empExtInfo.JobRoleId;
                int? apprGroupId = empExtInfo.ApprovalGroupId;

                if (jobroleid == 0)
                {
                    _logger.LogError("ApprovalsPendingForApprover JobRole Id is Invalid:" + id);
                    return NotFound(new RespStatus { Status = "Failure", Message = "JobRole Id is Invalid" });
                }

                var travelApprovalStatusTrackers = _context.TravelApprovalStatusTrackers
                               .Where(r =>
                                   (r.JobRoleId == jobroleid && r.ApprovalGroupId == apprGroupId && r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending && r.ProjManagerId == null)
                                   || (r.ProjManagerId == id && r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending)).ToList();

                CountOfApprovalsPending = CountOfApprovalsPending + travelApprovalStatusTrackers.Count;
            }


            return Ok();

        }


        [HttpGet("{id}")]
        [ActionName("ApprovalFlowForRequest")]
        public ActionResult<IEnumerable<ApprovalStatusFlowVM>> GetApprovalFlowForRequest(int id)
        {

            if (id == 0)
            {
                _logger.LogError("GetApprovalFlowForRequest - Id is not valid:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Travel Request Id is Invalid" });
            }



            var travelRequestStatusTrackers = _context.TravelApprovalStatusTrackers.Where(c => c.TravelApprovalRequestId == id).FirstOrDefault();


            if (travelRequestStatusTrackers == null)
            {
                _logger.LogError("Travel Request Status Tracker Request Id is returning null records:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Travel Request Request Id is Not Found" });
            }


            var ListOfTravelRequestStatusTrackers = _context.TravelApprovalStatusTrackers.Where(e => e.TravelApprovalRequestId == id).OrderBy(x => x.ApprovalLevelId).ToList();

            List<ApprovalStatusFlowVM> ListApprovalStatusFlow = new();

            if (ListOfTravelRequestStatusTrackers == null)
            {
                _logger.LogError("Travel Request Status tracker Id is returning null records:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Travel Request Status tracker Id is returning null records:" + id });
            }

            else
            {
                int reqEmpId = ListOfTravelRequestStatusTrackers[0].EmployeeId ?? 0;
                Employee reqEmp = _context.Employees.Find(reqEmpId);

                //add requester to the approval flow with Level 0
                ApprovalStatusFlowVM requesterInApprovalFlow = new();
                requesterInApprovalFlow.ApprovalLevel = 0;
                requesterInApprovalFlow.ApproverRole = "Requestor";
                requesterInApprovalFlow.ApproverName = reqEmp.GetFullName();
                requesterInApprovalFlow.ApproverActionDate = ListOfTravelRequestStatusTrackers[0].RequestDate;
                requesterInApprovalFlow.ApprovalStatusType = _context.ApprovalStatusTypes.Find((int)EApprovalStatus.Intitated).Status;


                ListApprovalStatusFlow.Add(requesterInApprovalFlow);
            }

            foreach (TravelApprovalStatusTracker statusTracker in ListOfTravelRequestStatusTrackers)
            {
                string claimApproverName = null;

                if (statusTracker.ProjectId > 0 && statusTracker.ProjectId != null)
                {
                    claimApproverName = _context.Employees.Where(e => e.Id == _context.Projects.Find(statusTracker.ProjectId).ProjectManagerId)
                        .Select(s => s.GetFullName()).FirstOrDefault();
                }
                else
                {
                    var approverExtInfo = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == statusTracker.JobRoleId && e.ApprovalGroupId == statusTracker.ApprovalGroupId).FirstOrDefault();
                    claimApproverName = _context.Employees.Find(approverExtInfo.EmployeeId).GetFullName();
                }

                ApprovalStatusFlowVM approvalStatusFlow = new();
                approvalStatusFlow.ApprovalLevel = statusTracker.ApprovalLevelId;

                if (statusTracker.ProjectId != null)
                {
                    approvalStatusFlow.ApproverRole = "Project Manager";
                }
                else
                {
                    approvalStatusFlow.ApproverRole = _context.JobRoles.Find(statusTracker.JobRoleId).GetJobRole();
                }
                approvalStatusFlow.ApproverName = claimApproverName;
                approvalStatusFlow.ApproverActionDate = statusTracker.ApproverActionDate;
                approvalStatusFlow.ApprovalStatusType = _context.ApprovalStatusTypes.Find(statusTracker.ApprovalStatusTypeId).Status;
                ListApprovalStatusFlow.Add(approvalStatusFlow);
            }

            return Ok(ListApprovalStatusFlow);

        }




        // POST: api/TravelApprovalStatusTrackers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TravelApprovalStatusTracker>> PostTravelApprovalStatusTracker(TravelApprovalStatusTracker travelApprovalStatusTracker)
        {
            _context.TravelApprovalStatusTrackers.Add(travelApprovalStatusTracker);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTravelApprovalStatusTracker", new { id = travelApprovalStatusTracker.Id }, travelApprovalStatusTracker);
        }

        // DELETE: api/TravelApprovalStatusTrackers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTravelApprovalStatusTracker(int id)
        {
            var travelApprovalStatusTracker = await _context.TravelApprovalStatusTrackers.FindAsync(id);
            if (travelApprovalStatusTracker == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Travel Approval Request Id invalid!" });
            }

            _context.TravelApprovalStatusTrackers.Remove(travelApprovalStatusTracker);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Travel Approval Request deleted!" });
        }

        //private bool TravelApprovalStatusTrackerExists(int id)
        //{
        //    return _context.TravelApprovalStatusTrackers.Any(e => e.Id == id);
        //}



    }
}
