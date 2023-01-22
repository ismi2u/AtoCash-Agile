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

namespace AtoCashAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User")]


    public class TravelApprovalRequestsController : ControllerBase
    {
        private readonly AtoCashDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<TravelApprovalRequestsController> _logger;
        public TravelApprovalRequestsController(AtoCashDbContext context, IEmailSender emailSender, ILogger<TravelApprovalRequestsController> logger)
        {
            this._context = context;
            this._emailSender = emailSender;
            _logger = logger;
        }


        // GET: api/TravelApprovalRequests
        [HttpGet]
        [ActionName("GetTravelApprovalRequests")]
        public async Task<ActionResult<IEnumerable<TravelApprovalRequestDTO>>> GetTravelApprovalRequests()
        {
            List<TravelApprovalRequestDTO> ListTravelApprovalRequestDTO = new();

            //var claimApprovalStatusTracker = await _context.TravelApprovalRequests.FindAsync(1);

            var TravelApprovalRequests = await _context.TravelApprovalRequests.ToListAsync();

            if (TravelApprovalRequests == null)
            {
                _logger.LogInformation("GetTravelApprovalRequests - null records");
            }

            foreach (TravelApprovalRequest travelApprovalRequest in TravelApprovalRequests)
            {
                TravelApprovalRequestDTO travelApprovalRequestDTO = new();

                travelApprovalRequestDTO.Id = travelApprovalRequest.Id;
                travelApprovalRequestDTO.EmployeeId = travelApprovalRequest.EmployeeId;
                travelApprovalRequestDTO.EmployeeName = _context.Employees.Find(travelApprovalRequest.EmployeeId).GetFullName();
                travelApprovalRequestDTO.TravelStartDate = travelApprovalRequest.TravelStartDate;
                travelApprovalRequestDTO.TravelEndDate = travelApprovalRequest.TravelEndDate;
                travelApprovalRequestDTO.TravelPurpose = travelApprovalRequest.TravelPurpose;
                travelApprovalRequestDTO.RequestDate = travelApprovalRequest.RequestDate;

                travelApprovalRequestDTO.BusinessTypeId = travelApprovalRequest.BusinessTypeId;
                travelApprovalRequestDTO.BusinessType = travelApprovalRequest.BusinessTypeId != null ? _context.BusinessTypes.Find(travelApprovalRequest.BusinessTypeId).BusinessTypeName : null;
                travelApprovalRequestDTO.BusinessUnitId = travelApprovalRequest.BusinessUnitId;
                travelApprovalRequestDTO.BusinessUnit = travelApprovalRequest.BusinessUnitId != null ? _context.BusinessUnits.Find(travelApprovalRequest.BusinessUnitId).GetBusinessUnitName() : null;


                if (travelApprovalRequest.BusinessUnitId != null)
                {
                    var locationId = _context.BusinessUnits.Find(travelApprovalRequest.BusinessUnitId).LocationId;
                    travelApprovalRequestDTO.Location = _context.Locations.Find(locationId).LocationName;
                }

                travelApprovalRequestDTO.CostCenterId = travelApprovalRequest.CostCenterId;
                travelApprovalRequestDTO.CostCenter = travelApprovalRequest.CostCenterId != null ? _context.CostCenters.Find(travelApprovalRequest.CostCenterId).GetCostCentre() : null;


                travelApprovalRequestDTO.ProjectId = travelApprovalRequest.ProjectId;
                travelApprovalRequestDTO.ProjectName = travelApprovalRequest.ProjectId != null ? _context.Projects.Find(travelApprovalRequest.ProjectId).ProjectName : null;
                travelApprovalRequestDTO.SubProjectId = travelApprovalRequest.SubProjectId;
                travelApprovalRequestDTO.SubProjectName = travelApprovalRequest.SubProjectId != null ? _context.SubProjects.Find(travelApprovalRequest.SubProjectId).SubProjectName : null;
                travelApprovalRequestDTO.WorkTaskId = travelApprovalRequest.WorkTaskId;
                travelApprovalRequestDTO.WorkTaskName = travelApprovalRequest.WorkTaskId != null ? _context.WorkTasks.Find(travelApprovalRequest.WorkTaskId).TaskName : null;
                travelApprovalRequestDTO.ApprovalStatusTypeId = travelApprovalRequest.ApprovalStatusTypeId;
                travelApprovalRequestDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(travelApprovalRequest.ApprovalStatusTypeId).Status;
                travelApprovalRequestDTO.ApproverActionDate = travelApprovalRequest.ApproverActionDate;


                ListTravelApprovalRequestDTO.Add(travelApprovalRequestDTO);
            }

            return ListTravelApprovalRequestDTO.OrderByDescending(o => o.RequestDate).ToList();
        }



        // GET: api/TravelApprovalRequests/5
        [HttpGet("{id}")]
        [ActionName("GetTravelApprovalRequest")]
        public async Task<ActionResult<TravelApprovalRequestDTO>> GetTravelApprovalRequest(int id)
        {


            var travelApprovalRequest = await _context.TravelApprovalRequests.FindAsync(id);

            if (travelApprovalRequest == null)
            {
                _logger.LogError("GetTravelApprovalRequest Request Id is not valid Id:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Travel Approval Request Id invalid!" });
            }
            TravelApprovalRequestDTO travelApprovalRequestDTO = new();


            travelApprovalRequestDTO.Id = travelApprovalRequest.Id;
            travelApprovalRequestDTO.EmployeeId = travelApprovalRequest.EmployeeId;
            travelApprovalRequestDTO.EmployeeName = _context.Employees.Find(travelApprovalRequest.EmployeeId).GetFullName();
            travelApprovalRequestDTO.TravelStartDate = travelApprovalRequest.TravelStartDate;
            travelApprovalRequestDTO.TravelEndDate = travelApprovalRequest.TravelEndDate;
            travelApprovalRequestDTO.TravelPurpose = travelApprovalRequest.TravelPurpose;
            travelApprovalRequestDTO.RequestDate = travelApprovalRequest.RequestDate;

            travelApprovalRequestDTO.BusinessTypeId = travelApprovalRequest.BusinessTypeId;
            travelApprovalRequestDTO.BusinessType = travelApprovalRequest.BusinessTypeId != null ? _context.BusinessTypes.Find(travelApprovalRequest.BusinessTypeId).BusinessTypeName : null;
            travelApprovalRequestDTO.BusinessUnitId = travelApprovalRequest.BusinessUnitId;
            travelApprovalRequestDTO.BusinessUnit = travelApprovalRequest.BusinessUnitId != null ? _context.BusinessUnits.Find(travelApprovalRequest.BusinessUnitId).GetBusinessUnitName() : null;
           

            if(travelApprovalRequest.BusinessUnitId != null)
            {
                var locationId = _context.BusinessUnits.Find(travelApprovalRequest.BusinessUnitId).LocationId;
                travelApprovalRequestDTO.Location = _context.Locations.Find(locationId).LocationName;
            }

            travelApprovalRequestDTO.CostCenterId = travelApprovalRequest.CostCenterId;
            travelApprovalRequestDTO.CostCenter = travelApprovalRequest.CostCenterId != null ? _context.CostCenters.Find(travelApprovalRequest.CostCenterId).GetCostCentre() : null;

            travelApprovalRequestDTO.ProjectId = travelApprovalRequest.ProjectId;
            travelApprovalRequestDTO.ProjectName = travelApprovalRequest.ProjectId != null ? _context.Projects.Find(travelApprovalRequest.ProjectId).ProjectName : null;
            travelApprovalRequestDTO.SubProjectId = travelApprovalRequest.SubProjectId;
            travelApprovalRequestDTO.SubProjectName = travelApprovalRequest.SubProjectId != null ? _context.SubProjects.Find(travelApprovalRequest.SubProjectId).SubProjectName : null;
            travelApprovalRequestDTO.WorkTaskId = travelApprovalRequest.WorkTaskId;
            travelApprovalRequestDTO.WorkTaskName = travelApprovalRequest.WorkTaskId != null ? _context.WorkTasks.Find(travelApprovalRequest.WorkTaskId).TaskName : null;
            travelApprovalRequestDTO.ApprovalStatusTypeId = travelApprovalRequest.ApprovalStatusTypeId;
            travelApprovalRequestDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(travelApprovalRequest.ApprovalStatusTypeId).Status;
            travelApprovalRequestDTO.ApproverActionDate = travelApprovalRequest.ApproverActionDate;

            travelApprovalRequestDTO.Comments = travelApprovalRequest.Comments;

            return travelApprovalRequestDTO;
        }





        [HttpGet("{id}")]
        [ActionName("GetTravelApprovalRequestRaisedForEmployee")]
        public async Task<ActionResult<IEnumerable<TravelApprovalRequestDTO>>> GetTravelApprovalRequestRaisedForEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                _logger.LogError("Travel : Employee Id is not valid:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id is Invalid!" });
            }

            var TravelApprovalRequests = await _context.TravelApprovalRequests.Where(p => p.EmployeeId == id).ToListAsync();

            if (TravelApprovalRequests == null)
            {
                _logger.LogError("Travel :TravelApprovalRequests is null");
                return Ok(new RespStatus { Status = "Failure", Message = "No Travel Requests raised!" });
            }

            List<TravelApprovalRequestDTO> TravelApprovalRequestDTOs = new();

            foreach (var travelApprovalRequest in TravelApprovalRequests)
            {
                TravelApprovalRequestDTO travelApprovalRequestDTO = new();


                travelApprovalRequestDTO.Id = travelApprovalRequest.Id;
                travelApprovalRequestDTO.EmployeeId = travelApprovalRequest.EmployeeId;
                travelApprovalRequestDTO.EmployeeName = _context.Employees.Find(travelApprovalRequest.EmployeeId).GetFullName();
                travelApprovalRequestDTO.TravelStartDate = travelApprovalRequest.TravelStartDate;
                travelApprovalRequestDTO.TravelEndDate = travelApprovalRequest.TravelEndDate;
                travelApprovalRequestDTO.TravelPurpose = travelApprovalRequest.TravelPurpose;
                travelApprovalRequestDTO.RequestDate = travelApprovalRequest.RequestDate;

                travelApprovalRequestDTO.BusinessTypeId = travelApprovalRequest.BusinessTypeId;
                travelApprovalRequestDTO.BusinessType = travelApprovalRequest.BusinessTypeId != null ? _context.BusinessTypes.Find(travelApprovalRequest.BusinessTypeId).BusinessTypeName : null;
                travelApprovalRequestDTO.BusinessUnitId = travelApprovalRequest.BusinessUnitId;
                travelApprovalRequestDTO.BusinessUnit = travelApprovalRequest.BusinessUnitId != null ? _context.BusinessUnits.Find(travelApprovalRequest.BusinessUnitId).GetBusinessUnitName() : null;


                if (travelApprovalRequest.BusinessUnitId != null)
                {
                    var locationId = _context.BusinessUnits.Find(travelApprovalRequest.BusinessUnitId).LocationId;
                    travelApprovalRequestDTO.Location = _context.Locations.Find(locationId).LocationName;
                }

                travelApprovalRequestDTO.CostCenterId = travelApprovalRequest.CostCenterId;
                travelApprovalRequestDTO.CostCenter = travelApprovalRequest.CostCenterId != null ? _context.CostCenters.Find(travelApprovalRequest.CostCenterId).GetCostCentre() : null;


                travelApprovalRequestDTO.ProjectId = travelApprovalRequest.ProjectId;
                travelApprovalRequestDTO.ProjectName = travelApprovalRequest.ProjectId != null ? _context.Projects.Find(travelApprovalRequest.ProjectId).ProjectName : null;
                travelApprovalRequestDTO.SubProjectId = travelApprovalRequest.SubProjectId;
                travelApprovalRequestDTO.SubProjectName = travelApprovalRequest.SubProjectId != null ? _context.SubProjects.Find(travelApprovalRequest.SubProjectId).SubProjectName : null;
                travelApprovalRequestDTO.WorkTaskId = travelApprovalRequest.WorkTaskId;
                travelApprovalRequestDTO.WorkTaskName = travelApprovalRequest.WorkTaskId != null ? _context.WorkTasks.Find(travelApprovalRequest.WorkTaskId).TaskName : null;
                travelApprovalRequestDTO.ApprovalStatusTypeId = travelApprovalRequest.ApprovalStatusTypeId;
                travelApprovalRequestDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(travelApprovalRequest.ApprovalStatusTypeId).Status;
                travelApprovalRequestDTO.ApproverActionDate = travelApprovalRequest.ApproverActionDate;




                // set the bookean flat to TRUE if No approver has yet approved the Request else FALSE
                bool ifAnyOfStatusRecordsApproved = _context.TravelApprovalStatusTrackers.Where(t =>
                                                         (t.ApprovalStatusTypeId == (int)EApprovalStatus.Rejected ||
                                                          t.ApprovalStatusTypeId == (int)EApprovalStatus.Approved) &&
                                                          t.TravelApprovalRequestId == travelApprovalRequest.Id).Any();

                travelApprovalRequestDTO.ShowEditDelete = ifAnyOfStatusRecordsApproved ? false : true;
                //;

                TravelApprovalRequestDTOs.Add(travelApprovalRequestDTO);
            }


            return Ok(TravelApprovalRequestDTOs.OrderByDescending(o => o.RequestDate).ToList());
        }


        [HttpGet("{id}")]
        [ActionName("CountAllTravelRequestRaisedByEmployee")]
        public async Task<ActionResult> CountAllTravelRequestRaisedByEmployee(int id)
        {

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return Ok(0);
            }

            var travelApprovalRequests = await _context.TravelApprovalRequests.Where(p => p.EmployeeId == id).ToListAsync();

            if (travelApprovalRequests == null)
            {
                return Ok(0);
            }

            int TotalCount = _context.TravelApprovalRequests.Where(c => c.EmployeeId == id).Count();
            int PendingCount = _context.TravelApprovalRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).Count();
            int RejectedCount = _context.TravelApprovalRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Rejected).Count();
            int ApprovedCount = _context.TravelApprovalRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Approved).Count();

            return Ok(new { TotalCount, PendingCount, RejectedCount, ApprovedCount });
        }


        [HttpGet]
        [ActionName("GetTravelReqInPendingForAll")]
        public async Task<ActionResult<int>> GetTravelReqInPendingForAll()
        {
            //debug
            var TravelApprovalRequests = await _context.TravelApprovalRequests.Include("TravelApprovalStatusTrackers").ToListAsync();


            //var TravelApprovalRequests = await _context.TravelApprovalRequests.Where(c => c.ApprovalStatusTypeId == ApprovalStatus.Pending).select( );

            if (TravelApprovalRequests == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Travel Approval Request Id invalid!" });
            }

            return Ok(TravelApprovalRequests.Count);
        }



        // PUT: api/TravelApprovalRequests/5
        [HttpPut("{id}")]
        [ActionName("PutTravelApprovalRequest")]

        public async Task<IActionResult> PutTravelApprovalRequest(int id, TravelApprovalRequestDTO travelApprovalRequestDTO)
        {
            if (id != travelApprovalRequestDTO.Id)
            {
                _logger.LogError("Travel Request: Travel Id is invalid - update failed");
                return Conflict(new RespStatus { Status = "Failure", Message = "Travel Id is invalid" });
            }

            var travelApprovalRequest = await _context.TravelApprovalRequests.FindAsync(id);

            ///update the Wallet of the employe to reflect the changes
            int ApprovedCount = _context.TravelApprovalStatusTrackers.Where(e => e.TravelApprovalRequestId == travelApprovalRequest.Id && e.ApprovalStatusTypeId == (int)EApprovalStatus.Approved).Count();
            if (ApprovedCount != 0)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Travel Requests cant be Edited after Approval!" });
            }


            travelApprovalRequest.Id = travelApprovalRequestDTO.Id;
            travelApprovalRequest.EmployeeId = travelApprovalRequestDTO.EmployeeId;
            travelApprovalRequest.TravelStartDate = travelApprovalRequestDTO.TravelStartDate;
            travelApprovalRequest.TravelEndDate = travelApprovalRequestDTO.TravelEndDate;
            travelApprovalRequest.TravelPurpose = travelApprovalRequestDTO.TravelPurpose;
            travelApprovalRequest.RequestDate = DateTime.UtcNow;
          
            if (travelApprovalRequestDTO.ProjectId != null)
            {
                travelApprovalRequest.ProjectId = travelApprovalRequestDTO.ProjectId;
                travelApprovalRequest.SubProjectId = travelApprovalRequestDTO.SubProjectId;
                travelApprovalRequest.WorkTaskId = travelApprovalRequestDTO.WorkTaskId;
            }
            else
            {
                travelApprovalRequest.BusinessTypeId = travelApprovalRequestDTO.BusinessTypeId;
                travelApprovalRequest.BusinessUnitId = travelApprovalRequestDTO.BusinessUnitId;
               
            }

            _context.TravelApprovalRequests.Update(travelApprovalRequest);


            //now update the TravelApprovalStatus Trackers
            var travelStatusTrackers = _context.TravelApprovalStatusTrackers.Where(e => e.TravelApprovalRequestId == travelApprovalRequest.Id).OrderBy(o => o.Id).ToList();
            bool IsFirstEmail = true;
            foreach (var travel in travelStatusTrackers)
            {

                TravelApprovalStatusTracker travelStatusItem = await _context.TravelApprovalStatusTrackers.FindAsync(travel.Id);


                travelStatusItem.TravelStartDate = travelApprovalRequestDTO.TravelStartDate;
                travelStatusItem.TravelEndDate = travelApprovalRequestDTO.TravelEndDate;

                if (travelApprovalRequestDTO.ProjectId != null)
                {
                    travelStatusItem.ProjectId = travelApprovalRequestDTO.ProjectId;
                    travelStatusItem.SubProjectId = travelApprovalRequestDTO.SubProjectId;
                    travelStatusItem.WorkTaskId = travelApprovalRequestDTO.WorkTaskId;
                }
                else
                {
                    travelStatusItem.BusinessTypeId = travelApprovalRequestDTO.BusinessTypeId;
                    travelStatusItem.BusinessUnitId = travelApprovalRequestDTO.BusinessUnitId;

                }
                  travelStatusItem.RequestDate = DateTime.UtcNow;

                _context.TravelApprovalStatusTrackers.Update(travelStatusItem);

                if (IsFirstEmail)
                {
                    _logger.LogInformation("Travel Approval Email Start");

                    string[] paths = { Directory.GetCurrentDirectory(), "TravelApprNotificationEmail.html" };
                    string FilePath = Path.Combine(paths);
                    _logger.LogInformation("Email template path " + FilePath);
                    StreamReader str = new StreamReader(FilePath);
                    string MailText = str.ReadToEnd();
                    str.Close();

                    var empExtendedInfo = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == travelStatusItem.JobRoleId && e.ApprovalGroupId == travelStatusItem.ApprovalGroupId).FirstOrDefault();
                    var approver = await _context.Employees.FindAsync(travelStatusItem.EmployeeId);

                    var approverMailAddress = approver.Email;
                    string subject = "(Modified) Travel Approval Request No# " + travelApprovalRequestDTO.Id.ToString();
                    Employee emp = await _context.Employees.FindAsync(travelApprovalRequestDTO.EmployeeId);
                    var travelreq = _context.TravelApprovalRequests.Find(travelApprovalRequestDTO.Id);

                    var builder = new MimeKit.BodyBuilder();

                    MailText = MailText.Replace("{Requester}", emp.GetFullName());
                    MailText = MailText.Replace("{ApproverName}", approver.GetFullName());
                    MailText = MailText.Replace("{Request}", travelreq.TravelStartDate.ToString() + " - " + travelreq.TravelEndDate.ToString() + " (Purpose): " + travelreq.TravelPurpose.ToString());
                    MailText = MailText.Replace("{RequestNumber}", travelreq.Id.ToString());
                    builder.HtmlBody = MailText;

                    var messagemail = new Message(new string[] { approverMailAddress }, subject, builder.HtmlBody);

                    await _emailSender.SendEmailAsync(messagemail);
                    _logger.LogInformation("Travel Request update Email Sent");

                    IsFirstEmail = false;
                }
            }



            //_context.Entry(travelApprovalRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogInformation("Travel Request update failed ");
                return BadRequest(new RespStatus { Status = "Failure", Message = "Travel Approval Request update failed!" });
            }

            return Ok(new RespStatus { Status = "Success", Message = "Travel Approval Request Updated!" });

        }

        // POST: api/TravelApprovalRequests
        [HttpPost]
        [ActionName("PostTravelApprovalRequest")]
        public async Task<ActionResult<TravelApprovalRequest>> PostTravelApprovalRequest(TravelApprovalRequestDTO travelApprovalRequestDTO)
        {
            //Step ##1

            ReturnIntAndResponseString SuccessResult = null;

            var dupReq = _context.TravelApprovalRequests.Where(
                t => t.TravelStartDate.Value == travelApprovalRequestDTO.TravelStartDate.Value &&
                t.TravelEndDate.Value == travelApprovalRequestDTO.TravelEndDate.Value &&
                t.EmployeeId == travelApprovalRequestDTO.EmployeeId &&
                t.TravelPurpose == travelApprovalRequestDTO.TravelPurpose).Count();

            if (dupReq != 0)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Duplicate request cannot be created" });
            }




            //##Step 2

            if (travelApprovalRequestDTO.ProjectId != null)
            {
                //Goes to Option 1 (Project)
                SuccessResult = await Task.Run(() => ProjectTravelRequest(travelApprovalRequestDTO));
            }
            else
            {
                //Goes to Option 2 (Business Unit)
                SuccessResult = await Task.Run(() => BusinessUnitTravelRequest(travelApprovalRequestDTO));
            }


            if (SuccessResult.IntReturn == 0)
            {
                _logger.LogInformation("PostExpenseReimburseRequest - Process completed");

                return Ok(new RespStatus { Status = "Success", Message = SuccessResult.StrResponse });
            }
            else
            {
                _logger.LogError("Expense Reimburse Request creation failed!");

                return BadRequest(new RespStatus { Status = "Failure", Message = SuccessResult.StrResponse });
            }

        }

        // DELETE: api/TravelApprovalRequests/5
        [HttpDelete("{id}")]
        [ActionName("DeleteTravelApprovalRequest")]
        public async Task<IActionResult> DeleteTravelApprovalRequest(int id)
        {
            var travelApprovalRequest = await _context.TravelApprovalRequests.FindAsync(id);
            if (travelApprovalRequest == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Travel Approval Request Id invalid!" });
            }

            var trvlappStatusTrackers = _context.TravelApprovalStatusTrackers.Where(c => c.TravelApprovalRequestId == travelApprovalRequest.Id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Approved).ToList();

            int ApprovedCount = trvlappStatusTrackers.Count;

            if (ApprovedCount > 0)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Travel Request cant be Deleted after Approval!" });
            }

            _context.TravelApprovalRequests.Remove(travelApprovalRequest);

            var travlapprStatusTrackers = _context.TravelApprovalStatusTrackers.Where(c => c.TravelApprovalRequestId == travelApprovalRequest.Id).ToList();

            foreach (var item in travlapprStatusTrackers)
            {
                _context.TravelApprovalStatusTrackers.Remove(item);
            }
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Travel Approval Request Deleted!" });
        }



        /// <summary>
        /// This is the option 1 : : PROJECT BASED TRAVEL REQUEST
        /// </summary>
        /// <param name="travelApprovalRequestDto"></param>
        /// <param name="travelApprovalRequestDto"></param>
        private async Task<ReturnIntAndResponseString> ProjectTravelRequest(TravelApprovalRequestDTO travelApprovalRequestDTO)
        {

            ReturnIntAndResponseString returnIntAndResponseString = new();
            _logger.LogInformation("ProjectBasedTravelRequest Started");
            #region
            int costCenterId = _context.Projects.Find(travelApprovalRequestDTO.ProjectId).CostCenterId;
            int projManagerid = _context.Projects.Find(travelApprovalRequestDTO.ProjectId).ProjectManagerId;
            var approver = _context.Employees.Find(projManagerid);
            int? reqEmpid = travelApprovalRequestDTO.EmployeeId;
            //int? maxApprLevel = _context.ApprovalRoleMaps.Max(a => a.ApprovalLevelId);

            //int empApprLevel = _context.ApprovalRoleMaps.Where(a => a.JobRoleId == _context.Employees.Find(reqEmpid).RoleId).FirstOrDefault().Id;
            bool isSelfApprovedRequest = false;
            #endregion


            if (approver != null)
            {
                _logger.LogInformation("Project Manager defined, no issues");
            }
            else
            {
                _logger.LogError("Project Manager is not Assigned");
                returnIntAndResponseString.IntReturn = 1;
                returnIntAndResponseString.StrResponse = "Project: Project Manager is not Assigned";
                return returnIntAndResponseString;
            }

            //### 1. If Employee Travel Request enter a record in TravelApprovalRequestTracker
            #region

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {
                TravelApprovalRequest travelApprovalRequest = new();

                travelApprovalRequest.Id = travelApprovalRequestDTO.Id;
                travelApprovalRequest.EmployeeId = travelApprovalRequestDTO.EmployeeId;
                travelApprovalRequest.TravelStartDate = travelApprovalRequestDTO.TravelStartDate;
                travelApprovalRequest.TravelEndDate = travelApprovalRequestDTO.TravelEndDate;
                travelApprovalRequest.TravelPurpose = travelApprovalRequestDTO.TravelPurpose;
                travelApprovalRequest.RequestDate = DateTime.UtcNow;
                travelApprovalRequest.BusinessTypeId = null;
                travelApprovalRequest.BusinessUnitId = null;
                travelApprovalRequest.ProjectId = travelApprovalRequestDTO.ProjectId;
                travelApprovalRequest.SubProjectId = travelApprovalRequestDTO.SubProjectId;
                travelApprovalRequest.WorkTaskId = travelApprovalRequestDTO.WorkTaskId;
                travelApprovalRequest.CostCenterId = _context.Projects.Find(travelApprovalRequestDTO.ProjectId).CostCenterId;
                travelApprovalRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Pending;
                travelApprovalRequest.Comments = "Travel Request In Process!";


                _context.TravelApprovalRequests.Add(travelApprovalRequest);

                await _context.SaveChangesAsync();
                //get the saved record Id
                travelApprovalRequestDTO.Id = travelApprovalRequest.Id;
                #endregion

                //##### 3. Add an entry to Travel Approval Status tracker
                //get costcenterID based on project
                #region

                ///////////////////////////// Check if self Approved Request /////////////////////////////

                //if highest approver is requesting Petty cash request himself
                if (projManagerid == reqEmpid)
                {
                    isSelfApprovedRequest = true;
                }
                //////////////////////////////////////////////////////////////////////////////////////////
                TravelApprovalStatusTracker travelApprovalStatusTracker = new();
                if (isSelfApprovedRequest)
                {
                    travelApprovalStatusTracker.EmployeeId = travelApprovalRequestDTO.EmployeeId;
                    travelApprovalStatusTracker.TravelApprovalRequestId = travelApprovalRequestDTO.Id;
                    travelApprovalStatusTracker.TravelStartDate = travelApprovalRequestDTO.TravelStartDate;
                    travelApprovalStatusTracker.TravelEndDate = travelApprovalRequestDTO.TravelEndDate;

                    travelApprovalStatusTracker.BusinessTypeId = null;
                    travelApprovalStatusTracker.BusinessUnitId = null;


                    travelApprovalStatusTracker.ProjManagerId = projManagerid;
                    travelApprovalStatusTracker.ProjectId = travelApprovalRequestDTO.ProjectId;
                    travelApprovalStatusTracker.JobRoleId = null;
                    travelApprovalStatusTracker.ApprovalGroupId = null;
                    travelApprovalStatusTracker.ApprovalLevelId = 2; // default approval level is 2 for Project based request
                    travelApprovalStatusTracker.RequestDate = DateTime.UtcNow;
                    travelApprovalStatusTracker.ApproverActionDate = DateTime.UtcNow;
                    travelApprovalStatusTracker.ApproverEmpId = reqEmpid;
                    travelApprovalStatusTracker.Comments = "Travel Request is Self Approved!";
                    travelApprovalStatusTracker.ApprovalStatusTypeId = (int)EApprovalStatus.Approved; //status tracker


                    _context.TravelApprovalStatusTrackers.Add(travelApprovalStatusTracker);
                    travelApprovalRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;  //1-Initiating; 2-Pending; 3-InReview; 4-Approved; 5-Rejected
                    travelApprovalRequest.Comments = "Approved";
                    _context.TravelApprovalRequests.Update(travelApprovalRequest);
                }
                else
                {


                    travelApprovalStatusTracker.EmployeeId = travelApprovalRequestDTO.EmployeeId;
                    travelApprovalStatusTracker.TravelApprovalRequestId = travelApprovalRequestDTO.Id;
                    travelApprovalStatusTracker.TravelStartDate = travelApprovalRequestDTO.TravelStartDate;
                    travelApprovalStatusTracker.TravelEndDate = travelApprovalRequestDTO.TravelEndDate;

                    travelApprovalStatusTracker.BusinessTypeId = null;
                    travelApprovalStatusTracker.BusinessUnitId = null;


                    travelApprovalStatusTracker.ProjManagerId = projManagerid;
                    travelApprovalStatusTracker.ProjectId = travelApprovalRequestDTO.ProjectId;
                    travelApprovalStatusTracker.JobRoleId = null;
                    travelApprovalStatusTracker.ApprovalGroupId = null;
                    travelApprovalStatusTracker.ApprovalLevelId = 2; // default approval level is 2 for Project based request
                    travelApprovalStatusTracker.RequestDate = DateTime.UtcNow;
                    travelApprovalStatusTracker.ApproverActionDate = null;
                    travelApprovalStatusTracker.ApproverEmpId = null;
                    travelApprovalStatusTracker.Comments = "Travel Request in Proceess"; ;
                    travelApprovalStatusTracker.ApprovalStatusTypeId = (int)EApprovalStatus.Pending; //status tracker


                    _context.TravelApprovalStatusTrackers.Add(travelApprovalStatusTracker);
                    _context.TravelApprovalRequests.Update(travelApprovalRequest);

                   
                }


                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation(approver.GetFullName() + " Status Tracker inserted");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Project: Status Tracker insert failed");
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Project: Status Tracker insert failed";
                    return returnIntAndResponseString;
                }
                #endregion
                

                //##### 4. Send email to the user
                //####################################
                #region
                if (isSelfApprovedRequest)
                {

                    returnIntAndResponseString.IntReturn = 0;
                    returnIntAndResponseString.StrResponse = "Project: Travel Approval request Created";

                    await AtoCashDbContextTransaction.CommitAsync();
                    return returnIntAndResponseString;
                }

                string[] paths = { Directory.GetCurrentDirectory(), "TravelApprNotificationEmail.html" };
                string FilePath = Path.Combine(paths);
                _logger.LogInformation("Email template path " + FilePath);
                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();


                _logger.LogInformation(approver.GetFullName() + " Email Start");
                var approverMailAddress = approver.Email;
                string subject = "Travel Approval Request No# " + travelApprovalRequestDTO.Id.ToString();
                Employee emp = await _context.Employees.FindAsync(travelApprovalRequestDTO.EmployeeId);
                var travelreq = _context.TravelApprovalRequests.Find(travelApprovalRequestDTO.Id);

                var builder = new MimeKit.BodyBuilder();

                MailText = MailText.Replace("{Requester}", emp.GetFullName());
                MailText = MailText.Replace("{ApproverName}", approver.GetFullName());
                MailText = MailText.Replace("{Request}", travelreq.TravelStartDate.ToString() + " - " + travelreq.TravelEndDate.ToString() + " (Purpose): " + travelreq.TravelPurpose.ToString());
                MailText = MailText.Replace("{RequestNumber}", travelreq.Id.ToString());
                builder.HtmlBody = MailText;

                var messagemail = new Message(new string[] { approverMailAddress }, subject, builder.HtmlBody);

                await _emailSender.SendEmailAsync(messagemail);
                #endregion

                _logger.LogInformation(approver.GetFullName() + " Email Sent");

                //await _context.SaveChangesAsync();

                await AtoCashDbContextTransaction.CommitAsync();
            }

            returnIntAndResponseString.IntReturn = 0;
            returnIntAndResponseString.StrResponse = "Project: Travel Approval request Created";
            return returnIntAndResponseString;
        }

        /// <summary>
        /// This is option 2 : Business Unit BASED CASH ADVANCE REQUEST
        /// </summary>
        /// <param name="travelApprovalRequestDto"></param>

        private async Task<ReturnIntAndResponseString> BusinessUnitTravelRequest(TravelApprovalRequestDTO travelApprovalRequestDto)
        {
            //### 1. If Employee Eligible for Cash Claim enter a record and reduce the available amount for next claim
            #region

            ReturnIntAndResponseString returnIntAndResponseString = new();
            _logger.LogInformation("Business Unit based Travel Request Started");

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {

                #region
                int? reqBussUnitId = travelApprovalRequestDto.BusinessUnitId;
                int costCenterId = _context.BusinessUnits.Find(reqBussUnitId).CostCenterId ?? 0;
                int? reqEmpid = travelApprovalRequestDto.EmployeeId;
                bool isSelfApprovedRequest = false;

                Employee reqEmp = _context.Employees.Find(reqEmpid);


                if (reqEmp == null)
                {
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Business: Employee Id invalid";
                    return returnIntAndResponseString;
                }
                EmployeeExtendedInfo reqEmpExtInfo = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == travelApprovalRequestDto.EmployeeId && e.BusinessUnitId == reqBussUnitId).FirstOrDefault();

                int? reqJobRoleId = reqEmpExtInfo.JobRoleId;
                int? reqApprGroupId = reqEmpExtInfo.ApprovalGroupId;


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


                _logger.LogInformation("All Approvers defined");

                TravelApprovalRequest travelApprovalRequest = new();
                travelApprovalRequest.EmployeeId = travelApprovalRequestDto.EmployeeId;
                travelApprovalRequest.TravelStartDate = travelApprovalRequestDto.TravelStartDate;
                travelApprovalRequest.TravelEndDate = travelApprovalRequestDto.TravelEndDate;
                travelApprovalRequest.TravelPurpose = travelApprovalRequestDto.TravelPurpose;
                travelApprovalRequest.RequestDate = DateTime.UtcNow;

                travelApprovalRequest.BusinessTypeId = travelApprovalRequestDto.BusinessTypeId;
                travelApprovalRequest.BusinessUnitId = travelApprovalRequestDto.BusinessUnitId;
                travelApprovalRequest.CostCenterId = costCenterId;


                travelApprovalRequest.ProjectId = travelApprovalRequestDto.ProjectId;
                travelApprovalRequest.SubProjectId = travelApprovalRequestDto.SubProjectId;
                travelApprovalRequest.WorkTaskId = travelApprovalRequestDto.WorkTaskId;
                travelApprovalRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Pending;
                travelApprovalRequest.ApproverActionDate = null;
                travelApprovalRequest.Comments = "travel Approval Request in Process!";
                _logger.LogInformation("travel Approval Request insert started");


                _context.TravelApprovalRequests.Add(travelApprovalRequest); //  <= this generated the Id
               

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Travel request Table inserted successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Travel request Table insert failed");
                    returnIntAndResponseString.IntReturn = 1;
                    returnIntAndResponseString.StrResponse = "Travel Approval request creation failed";
                    return returnIntAndResponseString;
                }

                //get the saved record Id
                travelApprovalRequestDto.Id = travelApprovalRequest.Id;

                ///////////////////////////// Check if self Approved Request /////////////////////////////

                //if highest approver is requesting Expense Reimburse request himself
                if (maxApprLevel == reqApprLevel)
                {
                    isSelfApprovedRequest = true;
                }
                //////////////////////////////////////////////////////////////////////////////////////////
                //var test = _context.ApprovalRoleMaps.Include(a => a.ApprovalLevel).ToList().OrderBy(o => o.ApprovalLevel.Level);

                var getEmpClaimApproversAllLevels = _context.ApprovalRoleMaps
                                       .Include(a => a.ApprovalLevel)
                                       .Where(a => a.ApprovalGroupId == reqApprGroupId)
                                       .OrderBy(o => o.ApprovalLevel.Level).ToList();
                bool isFirstApprover = true;
                _logger.LogInformation("Business Unit: Expense Reimburse request status tracker insert start");
                if (isSelfApprovedRequest)
                {
                    _logger.LogInformation("Self Approved Request");
                    TravelApprovalStatusTracker travelApprovalStatusTracker = new();


                    travelApprovalStatusTracker.EmployeeId = travelApprovalRequestDto.EmployeeId;
                    travelApprovalStatusTracker.TravelApprovalRequestId = travelApprovalRequestDto.Id; //Get 
                    travelApprovalStatusTracker.TravelStartDate = travelApprovalRequestDto.TravelStartDate;
                    travelApprovalStatusTracker.TravelEndDate = travelApprovalRequestDto.TravelEndDate;

                    travelApprovalStatusTracker.BusinessTypeId = travelApprovalRequestDto.BusinessTypeId;
                    travelApprovalStatusTracker.BusinessUnitId = travelApprovalRequestDto.BusinessUnitId;

                    travelApprovalStatusTracker.ProjectId = null;
                    travelApprovalStatusTracker.JobRoleId = reqJobRoleId;

                    travelApprovalStatusTracker.ApproverEmpId = null;
                    travelApprovalStatusTracker.ApprovalLevelId = reqApprLevel;
                    travelApprovalStatusTracker.ApprovalGroupId = reqApprGroupId;
                    travelApprovalStatusTracker.RequestDate = DateTime.UtcNow;
                    travelApprovalStatusTracker.ApproverActionDate = DateTime.UtcNow;

                    travelApprovalStatusTracker.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                    travelApprovalStatusTracker.Comments = "Approved";


                    _context.TravelApprovalStatusTrackers.Add(travelApprovalStatusTracker);
                    travelApprovalRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                    travelApprovalRequest.Comments = "Approved";
                    _context.TravelApprovalRequests.Update(travelApprovalRequest);
                
                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("BusinessUnit: travelApprovalRequest insert success");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Self approved travelApprovalRequest update failed");
                        returnIntAndResponseString.IntReturn = 1;
                        returnIntAndResponseString.StrResponse = "Self approved travelApprovalRequest update failed";
                        return returnIntAndResponseString;
                    }

                    _logger.LogInformation("Self Approved:Expense table Updated with Approved Status");
                }
                else
                {
                    foreach (ApprovalRoleMap ApprMap in getEmpClaimApproversAllLevels)
                    {

                        int? apprjobRoleId = ApprMap.JobRoleId;
                        var empExtInfo = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == apprjobRoleId && e.ApprovalGroupId == reqApprGroupId).FirstOrDefault();
                        int? approverEmpId = empExtInfo.EmployeeId;

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

                        _logger.LogInformation(approver.GetFullName() + " Status Tracker started");
                        TravelApprovalStatusTracker travelApprovalStatusTracker = new();

                        travelApprovalStatusTracker.EmployeeId = travelApprovalRequestDto.EmployeeId;
                        travelApprovalStatusTracker.TravelApprovalRequestId = travelApprovalRequestDto.Id; //Get 
                        travelApprovalStatusTracker.TravelStartDate = travelApprovalRequestDto.TravelStartDate;
                        travelApprovalStatusTracker.TravelEndDate = travelApprovalRequestDto.TravelEndDate;

                        travelApprovalStatusTracker.BusinessTypeId = travelApprovalRequestDto.BusinessTypeId;
                        travelApprovalStatusTracker.BusinessUnitId = travelApprovalRequestDto.BusinessUnitId;

                        travelApprovalStatusTracker.ProjectId = null;
                        travelApprovalStatusTracker.JobRoleId = apprjobRoleId;
                        travelApprovalStatusTracker.ApprovalLevelId = approverLevelid;
                        travelApprovalStatusTracker.ApprovalGroupId = reqApprGroupId;

                        travelApprovalStatusTracker.RequestDate = DateTime.UtcNow;
                        travelApprovalStatusTracker.ApproverActionDate = null;

                        travelApprovalStatusTracker.ApprovalStatusTypeId = isFirstApprover ? (int)EApprovalStatus.Pending : (int)EApprovalStatus.Intitated;
                        travelApprovalStatusTracker.Comments = "Awaiting Approver Action";

                      
                        _context.TravelApprovalStatusTrackers.Add(travelApprovalStatusTracker);
                        try
                        {
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("BusinessUnit: travelApprovalStatusTracker insert success");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Self approved travelApprovalStatusTracker update failed");
                            returnIntAndResponseString.IntReturn = 1;
                            returnIntAndResponseString.StrResponse = "Self approved travelApprovalRequest update failed";
                            return returnIntAndResponseString;
                        }

                        _logger.LogInformation(approver.GetFullName() + " Status Tracker inserted");
                        //##### 5. Send email to the Approver
                        //####################################

                        if (isFirstApprover)
                        {
                            _logger.LogInformation(approver.GetFullName() + " Email Start");

                            string[] paths = { Directory.GetCurrentDirectory(), "TravelApprNotificationEmail.html" };
                            string FilePath = Path.Combine(paths);
                            _logger.LogInformation("Email template path " + FilePath);
                            StreamReader str = new StreamReader(FilePath);
                            string MailText = str.ReadToEnd();
                            str.Close();

                            var approverMailAddress = approver.Email;
                            string subject = "Travel Approval Request No# " + travelApprovalRequestDto.Id.ToString();
                            Employee emp = await _context.Employees.FindAsync(travelApprovalRequestDto.EmployeeId);
                            var travelreq = _context.TravelApprovalRequests.Find(travelApprovalRequestDto.Id);

                            var builder = new MimeKit.BodyBuilder();

                            MailText = MailText.Replace("{Requester}", emp.GetFullName());
                            MailText = MailText.Replace("{ApproverName}", approver.GetFullName());
                            MailText = MailText.Replace("{Request}", travelreq.TravelStartDate.ToString() + " - " + travelreq.TravelEndDate.ToString() + " (Purpose): " + travelreq.TravelPurpose.ToString());
                            MailText = MailText.Replace("{RequestNumber}", travelreq.Id.ToString());
                            builder.HtmlBody = MailText;

                            var messagemail = new Message(new string[] { approverMailAddress }, subject, builder.HtmlBody);

                            await _emailSender.SendEmailAsync(messagemail);
                            _logger.LogInformation(approver.GetFullName() + " Email Sent");
                        }
                        isFirstApprover = false;

                        //repeat for each approver
                    }

                }

                await AtoCashDbContextTransaction.CommitAsync();
            }
            _logger.LogInformation("Business Unit: Travel Request Created successfully");
            returnIntAndResponseString.IntReturn = 0;
            returnIntAndResponseString.StrResponse = "Business Unit: Travel Request Created successfully";
            return returnIntAndResponseString;



        }
        #endregion



    }
}
#endregion