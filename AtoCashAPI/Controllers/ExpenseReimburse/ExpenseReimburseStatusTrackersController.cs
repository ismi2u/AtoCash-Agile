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

namespace AtoCashAPI.Controllers.ExpenseReimburse
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User")]
    public class ExpenseReimburseStatusTrackersController : ControllerBase
    {
        private readonly AtoCashDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExpenseReimburseStatusTrackersController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExpenseReimburseStatusTrackersController(AtoCashDbContext context,
                                                        IEmailSender emailSender,
                                                        ILogger<ExpenseReimburseStatusTrackersController> logger,
                                                        UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        // GET: api/ExpenseReimburseStatusTrackers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseReimburseStatusTracker>>> GetExpenseReimburseStatusTrackers()
        {
            return await _context.ExpenseReimburseStatusTrackers.ToListAsync();
        }

        // GET: api/ExpenseReimburseStatusTrackers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseReimburseStatusTracker>> GetExpenseReimburseStatusTracker(int id)
        {
            var expenseReimburseStatusTracker = await _context.ExpenseReimburseStatusTrackers.FindAsync(id);

            if (expenseReimburseStatusTracker == null)
            {
                _logger.LogError("expenseReimburseStatusTracker Id is not valid:" + id);
                return NotFound();
            }

            return expenseReimburseStatusTracker;
        }


        [HttpGet("{id}")]
        [ActionName("ApprovalFlowForRequest")]
        public ActionResult<IEnumerable<ApprovalStatusFlowVM>> GetApprovalFlowForRequest(int id)
        {

            if (id == 0)
            {
                _logger.LogError("GetApprovalFlowForRequest - Id is not valid:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Expense-Reimburse Request Id is Invalid" });
            }


            var expenseReimburseStatusTrackers = _context.ExpenseReimburseStatusTrackers.Where(e => e.ExpenseReimburseRequestId == id).FirstOrDefault();


            if (expenseReimburseStatusTrackers == null)
            {
                _logger.LogError("Expense-Reimburse Status Tracker Request Id is returning null records:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Expense-Reimburse Request Id is Not Found" });
            }


            var ListOfExpReimStatusTrackers = _context.ExpenseReimburseStatusTrackers.Where(e => e.ExpenseReimburseRequestId == id).OrderBy(x => x.JobRoleId).ToList();

            List<ApprovalStatusFlowVM> ListApprovalStatusFlow = new();

            if (ListOfExpReimStatusTrackers == null)
            {
                _logger.LogError("Expense-Reimburse Status Tracker Request Id is returning null records:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Status Tracker Request Id is returning null records:" + id });
            }

            else
            {
                int reqEmpId = ListOfExpReimStatusTrackers[0].EmployeeId ?? 0;
                Employee reqEmp = _context.Employees.Find(reqEmpId);

                //add requester to the approval flow with Level 0
                ApprovalStatusFlowVM requesterInApprovalFlow = new();
                requesterInApprovalFlow.ApprovalLevel = 0;
                requesterInApprovalFlow.ApproverRole = "Requestor";
                requesterInApprovalFlow.ApproverName = reqEmp.GetFullName();
                requesterInApprovalFlow.ApproverActionDate = ListOfExpReimStatusTrackers[0].RequestDate;
                requesterInApprovalFlow.ApprovalStatusType = _context.ApprovalStatusTypes.Find((int)EApprovalStatus.Intitated).Status;


                ListApprovalStatusFlow.Add(requesterInApprovalFlow);
            }

            foreach (ExpenseReimburseStatusTracker statusTracker in ListOfExpReimStatusTrackers)
            {
                string claimApproverName = null;

                if (statusTracker.ProjectId > 0)
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


        [HttpGet("{id}")]
        [ActionName("ApprovalsPendingForApprover")]
        public ActionResult<IEnumerable<CashAdvanceStatusTrackerDTO>> ApprovalsPendingForApprover(int id)
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

            List<ExpenseReimburseStatusTrackerDTO> ListExpenseReimburseStatusTrackerDTO = new();

            foreach ( EmployeeExtendedInfo empExtInfo in listEmpExtendedInfo)
            {
                int? jobRoleid = empExtInfo.JobRoleId;
                int? apprGroupId = empExtInfo.ApprovalGroupId;

                if (jobRoleid == 0)
                {
                    _logger.LogError("ApprovalsPendingForApprover JobRole Id is Invalid:" + id);
                    return Conflict(new RespStatus { Status = "Failure", Message = "JobRole Id is Invalid" });
                }

                var expenseReimburseStatusTrackers = _context.ExpenseReimburseStatusTrackers
                                    .Where(r =>
                                        (r.JobRoleId == jobRoleid && r.ApprovalGroupId == apprGroupId && r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending && r.ProjManagerId == null)
                                        || (r.ProjManagerId == id && r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending)).ToList();

               

                foreach (ExpenseReimburseStatusTracker expenseReimburseStatusTracker in expenseReimburseStatusTrackers)
                {
                    ExpenseReimburseStatusTrackerDTO expenseReimburseStatusTrackerDTO = new();

                    expenseReimburseStatusTrackerDTO.Id = expenseReimburseStatusTracker.Id;
                    expenseReimburseStatusTrackerDTO.EmployeeId = expenseReimburseStatusTracker.EmployeeId;
                    expenseReimburseStatusTrackerDTO.EmployeeName = _context.Employees.Find(expenseReimburseStatusTracker.EmployeeId).GetFullName();
                    expenseReimburseStatusTrackerDTO.ExpenseReimburseRequestId = expenseReimburseStatusTracker.ExpenseReimburseRequestId;
                    expenseReimburseStatusTrackerDTO.TotalClaimAmount = expenseReimburseStatusTracker.TotalClaimAmount;

                    expenseReimburseStatusTrackerDTO.BusinessTypeId = expenseReimburseStatusTracker.BusinessTypeId;
                    expenseReimburseStatusTrackerDTO.BusinessType = expenseReimburseStatusTracker.BusinessTypeId != null ? _context.BusinessTypes.Find(expenseReimburseStatusTracker.BusinessTypeId).BusinessTypeName : null;
                    expenseReimburseStatusTrackerDTO.BusinessUnitId = expenseReimburseStatusTracker.BusinessUnitId;
                    expenseReimburseStatusTrackerDTO.BusinessUnit = expenseReimburseStatusTracker.BusinessUnitId != null ? _context.BusinessUnits.Find(expenseReimburseStatusTracker.BusinessUnitId).GetBusinessUnitName() : null;


                    expenseReimburseStatusTrackerDTO.ProjectId = expenseReimburseStatusTracker.ProjectId;
                    expenseReimburseStatusTrackerDTO.Project = expenseReimburseStatusTracker.ProjectId != null ? _context.Projects.Find(expenseReimburseStatusTracker.ProjectId).ProjectName : null;
                    expenseReimburseStatusTrackerDTO.JobRoleId = expenseReimburseStatusTracker.JobRoleId;
                    expenseReimburseStatusTrackerDTO.JobRole = _context.JobRoles.Find(expenseReimburseStatusTracker.JobRoleId).GetJobRole();
                    expenseReimburseStatusTrackerDTO.ApprovalLevelId = expenseReimburseStatusTracker.ApprovalLevelId;
                    expenseReimburseStatusTrackerDTO.RequestDate = expenseReimburseStatusTracker.RequestDate;

                    //var apprEmpId = _context.EmployeeExtendedInfos.Where(e => e.ApprovalGroupId == expenseReimburseStatusTracker.ApprovalGroupId && e.JobRoleId == expenseReimburseStatusTracker.JobRoleId).FirstOrDefault().EmployeeId;

                    //expenseReimburseStatusTrackerDTO.ApproverName = _context.Employees.Find(apprEmpId).GetFullName();
                    expenseReimburseStatusTrackerDTO.ApproverActionDate = expenseReimburseStatusTracker.ApproverActionDate;
                    expenseReimburseStatusTrackerDTO.ApprovalStatusTypeId = expenseReimburseStatusTracker.ApprovalStatusTypeId;
                    expenseReimburseStatusTrackerDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(expenseReimburseStatusTracker.ApprovalStatusTypeId).Status;
                    expenseReimburseStatusTrackerDTO.Comments = expenseReimburseStatusTracker.Comments;


                    ListExpenseReimburseStatusTrackerDTO.Add(expenseReimburseStatusTrackerDTO);

                }

            }


            return Ok(ListExpenseReimburseStatusTrackerDTO.OrderByDescending(o => o.RequestDate).ToList());

        }



        //To get the counts of pending approvals

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

                var expenseReimburseStatusTrackers = _context.ExpenseReimburseStatusTrackers
                                   .Where(r =>
                                       (r.JobRoleId == jobroleid && r.ApprovalGroupId == apprGroupId && r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending && r.ProjManagerId == null)
                                       || (r.ProjManagerId == id && r.ApprovalStatusTypeId == (int)EApprovalStatus.Pending)).ToList();

                CountOfApprovalsPending = CountOfApprovalsPending + expenseReimburseStatusTrackers.Count;
            }

            return Ok(CountOfApprovalsPending);

        }

        // PUT: api/ExpenseReimburseStatusTrackers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutExpenseReimburseStatusTracker(List<ExpenseReimburseStatusTrackerDTO> ListExpenseReimburseStatusTrackerDto)
        {


            if (ListExpenseReimburseStatusTrackerDto.Count == 0)
            {
                _logger.LogError("ListExpenseReimburseStatusTrackerDto count is 0, no object to loop");
                return Conflict(new RespStatus { Status = "Failure", Message = "No Request to Approve!" });
            }


            bool isNextApproverAvailable = true;
            bool bRejectMessage = false;
            ApplicationUser? user = await _userManager.GetUserAsync(HttpContext.User);
            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {
                _logger.LogInformation("PutExpsensReimburseStatus Tracker record updation START");
                foreach (ExpenseReimburseStatusTrackerDTO expenseReimburseStatusTrackerDto in ListExpenseReimburseStatusTrackerDto)
                {
                    var expenseReimburseStatusTracker = await _context.ExpenseReimburseStatusTrackers.FindAsync(expenseReimburseStatusTrackerDto.Id);

                    //if same status continue to next loop, otherwise process
                    if (expenseReimburseStatusTracker.ApprovalStatusTypeId == expenseReimburseStatusTrackerDto.ApprovalStatusTypeId)
                    {
                        continue;
                    }

                    if (expenseReimburseStatusTrackerDto.ApprovalStatusTypeId == (int)EApprovalStatus.Rejected)
                    {
                        bRejectMessage = true;
                    }

                    


                    expenseReimburseStatusTracker.Id = expenseReimburseStatusTrackerDto.Id;
                    expenseReimburseStatusTracker.EmployeeId = expenseReimburseStatusTrackerDto.EmployeeId;
                    expenseReimburseStatusTracker.ExpenseReimburseRequestId = expenseReimburseStatusTrackerDto.ExpenseReimburseRequestId;
                    expenseReimburseStatusTracker.TotalClaimAmount = expenseReimburseStatusTrackerDto.TotalClaimAmount;

                    expenseReimburseStatusTracker.BusinessTypeId = expenseReimburseStatusTrackerDto.BusinessTypeId;
                    expenseReimburseStatusTracker.BusinessUnitId = expenseReimburseStatusTrackerDto.BusinessUnitId;

                    expenseReimburseStatusTracker.ProjectId = expenseReimburseStatusTrackerDto.ProjectId;
                    expenseReimburseStatusTracker.JobRoleId = expenseReimburseStatusTrackerDto.JobRoleId;
                    expenseReimburseStatusTracker.ApprovalLevelId = expenseReimburseStatusTrackerDto.ApprovalLevelId;
                    expenseReimburseStatusTracker.RequestDate = expenseReimburseStatusTrackerDto.RequestDate;

                    expenseReimburseStatusTracker.ApproverActionDate = DateTime.UtcNow;
                    expenseReimburseStatusTracker.ApproverEmpId = user != null ? user.EmployeeId : null;
                    expenseReimburseStatusTracker.Comments = bRejectMessage ? expenseReimburseStatusTrackerDto.Comments : "Approved";
                    expenseReimburseStatusTracker.ApprovalStatusTypeId = expenseReimburseStatusTrackerDto.ApprovalStatusTypeId;
                    


                    ExpenseReimburseStatusTracker claimitem;


                    if (expenseReimburseStatusTrackerDto.ProjectId != null)
                    {
                        _logger.LogInformation("Project based Expense update");
                        //final approver hence update Expense Reimburse request claim
                        claimitem = _context.ExpenseReimburseStatusTrackers.Where(c => c.ExpenseReimburseRequestId == expenseReimburseStatusTracker.ExpenseReimburseRequestId &&
                                    c.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).FirstOrDefault();
                        expenseReimburseStatusTracker.ApprovalStatusTypeId = expenseReimburseStatusTrackerDto.ApprovalStatusTypeId;
                        //DisbursementAndClaimsMaster update the record to Approved (ApprovalStatusId
                        int disbAndClaimItemId = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == claimitem.ExpenseReimburseRequestId).FirstOrDefault().Id;
                        var disbAndClaimItem = await _context.DisbursementsAndClaimsMasters.FindAsync(disbAndClaimItemId);

                        /// #############################
                        //   Crediting back to the wallet 
                        /// #############################
                        double? expenseReimAmt = claimitem.TotalClaimAmount;

                        

                        double? RoleLimitAmt = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == claimitem.EmployeeId).FirstOrDefault().MaxCashAdvanceLimit;
                        EmpCurrentCashAdvanceBalance empCurrentCashAdvanceBalance = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == claimitem.EmployeeId).FirstOrDefault();
                        double? empCurPettyBal = empCurrentCashAdvanceBalance.CurBalance;

                        //logic goes here

                        if (expenseReimAmt + empCurPettyBal >= RoleLimitAmt) // claiming amount is greater than replishable amount
                        {
                            disbAndClaimItem.AmountToWallet = RoleLimitAmt - empCurPettyBal;
                            disbAndClaimItem.AmountToCredit = expenseReimAmt - (RoleLimitAmt - empCurPettyBal);
                        }
                        else
                        {
                            //fully credit to Wallet - Zero amount to bank amount
                            disbAndClaimItem.AmountToWallet = expenseReimAmt;
                            disbAndClaimItem.AmountToCredit = 0;
                        }

                        _logger.LogInformation("Crediting to the wallet and change the status to approved");

                        disbAndClaimItem.ApprovalStatusId = bRejectMessage ? (int)EApprovalStatus.Rejected : (int)EApprovalStatus.Approved;
                        _context.Update(disbAndClaimItem);

                        _logger.LogInformation("Project based Expense update");

                        ////Final Approveer hence update the EmpCurrentPettyCashBalance table for the employee to reflect the credit
                        //empCurrentPettyCashBalance.CurBalance = empCurPettyBal + disbAndClaimItem.AmountToWallet ?? 0;
                        //_context.EmpCurrentPettyCashBalances.Update(empCurrentPettyCashBalance);

                        /////
                        ///


                        //Update ExpenseReimburseRequests table to update the record to Approved as the final approver has approved it.
                        int expenseReimReqId = _context.ExpenseReimburseRequests.Where(d => d.Id == claimitem.ExpenseReimburseRequestId).FirstOrDefault().Id;
                        var expenseReimReq = await _context.ExpenseReimburseRequests.FindAsync(expenseReimReqId);

                        expenseReimReq.ApprovalStatusTypeId = bRejectMessage ? (int)EApprovalStatus.Rejected : (int)EApprovalStatus.Approved;
                        expenseReimReq.Comments = bRejectMessage ? expenseReimburseStatusTrackerDto.Comments : "Approved";
                        expenseReimReq.ApproverActionDate = DateTime.UtcNow;
                        _context.Update(expenseReimReq);
                    }
                    else
                    {

                        //Check if the record is already approved
                        //if it is not approved then trigger next approver level email & Change the status to approved
                        if (expenseReimburseStatusTrackerDto.ApprovalStatusTypeId == (int)EApprovalStatus.Approved)
                        {
                            //Get the next approval level (get its ID)
                            //int qExpReimRequestId = expenseReimburseStatusTrackerDto.ExpenseReimburseRequestId ?? 0;
                            int? qExpReimRequestId = expenseReimburseStatusTrackerDto.ExpenseReimburseRequestId;

                            isNextApproverAvailable = true;

                            int CurClaimApprovalLevel = _context.ApprovalLevels.Find(expenseReimburseStatusTrackerDto.ApprovalLevelId).Level;
                            int nextClaimApprovalLevel = CurClaimApprovalLevel + 1;
                            int qApprovalLevelId;
                            int? apprGroupId = expenseReimburseStatusTracker.ApprovalGroupId;


                            if (_context.ApprovalRoleMaps.Where(a => a.ApprovalGroupId == apprGroupId && a.ApprovalLevelId == nextClaimApprovalLevel).FirstOrDefault() != null)
                            {
                                qApprovalLevelId = _context.ApprovalLevels.Where(x => x.Level == nextClaimApprovalLevel).FirstOrDefault().Id;
                            }
                            else
                            {
                                qApprovalLevelId = _context.ApprovalLevels.Where(x => x.Level == CurClaimApprovalLevel).FirstOrDefault().Id;
                                isNextApproverAvailable = false;
                            }


                            int qApprovalStatusTypeId = isNextApproverAvailable ? (int)EApprovalStatus.Intitated: (int)EApprovalStatus.Pending;

                            //update the next level approver Track request to PENDING (from Initiating) 
                            //if claimitem is not null change the status
                            if (isNextApproverAvailable)
                            {
                               
                                    claimitem = _context.ExpenseReimburseStatusTrackers.Where(c => c.ExpenseReimburseRequestId == qExpReimRequestId &&
                                   c.ApprovalStatusTypeId == qApprovalStatusTypeId &&
                                    c.ApprovalGroupId == apprGroupId &&
                                   c.ApprovalLevelId == qApprovalLevelId).FirstOrDefault();
                              

                                if (claimitem != null)
                                {
                                    claimitem.ApprovalStatusTypeId = (int)EApprovalStatus.Pending;
                                }
                                else
                                {
                                    _logger.LogError("DisbursementAndClaims table has no record for  ExpenseReimburseRequestId:" + qExpReimRequestId);
                                }

                            }
                            else
                            {
                                //final approver hence tally against PettyCashRequest

                               
                                    claimitem = _context.ExpenseReimburseStatusTrackers.Where(c => c.ExpenseReimburseRequestId == qExpReimRequestId &&
                                   c.ApprovalStatusTypeId == qApprovalStatusTypeId &&
                                    c.ApprovalGroupId == apprGroupId &&
                                   c.ApprovalLevelId == qApprovalLevelId).FirstOrDefault();
                    
                                //claimitem.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                                claimitem.ApproverActionDate = DateTime.UtcNow;
                                _logger.LogInformation("DisbursementAndClaims table updated as approved for ExpenseReimburseRequestId:" + qExpReimRequestId);

                                //final Approver hence updating ExpenseReimburseRequest table
                                var expenseReimburseRequest = _context.ExpenseReimburseRequests.Find(qExpReimRequestId);
                                expenseReimburseRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                                expenseReimburseRequest.ApproverActionDate = DateTime.UtcNow;
                                expenseReimburseRequest.Comments = bRejectMessage ? expenseReimburseStatusTrackerDto.Comments : "Approved";
                                _context.Update(expenseReimburseRequest);


                                //DisbursementAndClaimsMaster update the record to Approved (ApprovalStatusId
                                int disbAndClaimItemId = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == claimitem.ExpenseReimburseRequestId).FirstOrDefault().Id;
                                var disbAndClaimItem = await _context.DisbursementsAndClaimsMasters.FindAsync(disbAndClaimItemId);

                                /// #############################
                                //   Crediting back to the wallet 
                                /// #############################
                                /// 
                                _logger.LogInformation("============== Crediting to Wallet =======================");
                                double? expenseReimAmt = expenseReimburseRequest.TotalClaimAmount;
                                double? RoleLimitAmt = _context.EmpCurrentCashAdvanceBalances.Where(e=> e.EmployeeId == claimitem.EmployeeId).FirstOrDefault().MaxCashAdvanceLimit;
                                EmpCurrentCashAdvanceBalance empCurrentCashAdvanceBalance = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == expenseReimburseRequest.EmployeeId).FirstOrDefault();
                                double? empCurPettyBal = empCurrentCashAdvanceBalance.CurBalance;
                                double? empCashOnHand = empCurrentCashAdvanceBalance.CashOnHand;

                                //Do we credit to wallet when pending cash request are still Un-Approved state (which could be rejected)

                                double pendingTotalPettCashRequestAmount = _context.CashAdvanceRequests.Where(p => p.EmployeeId == expenseReimburseRequest.EmployeeId && p.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).Select(s => s.CashAdvanceAmount).Sum() ?? 0;


                                if (empCashOnHand >= expenseReimAmt)
                                {
                                    disbAndClaimItem.AmountToCredit = 0;
                                    disbAndClaimItem.AmountToWallet = expenseReimAmt;

                                    //empCurrentPettyCashBalance.CashOnHand = empCurrentPettyCashBalance.CashOnHand - expenseReimAmt;
                                    //.CurBalance = empCurrentPettyCashBalance.CurBalance + expenseReimAmt;
                                }
                                else if (empCashOnHand > 0)
                                {
                                    disbAndClaimItem.AmountToCredit = expenseReimAmt - empCashOnHand;
                                    disbAndClaimItem.AmountToWallet = empCashOnHand;

                                    //should not update the empCurrentPettyCashBalance as it may be rejected 
                                    //empCurrentPettyCashBalance.CashOnHand = empCurrentPettyCashBalance.CashOnHand - (expenseReimAmt - empCashOnHand);
                                    // empCurrentPettyCashBalance.CurBalance = empCurrentPettyCashBalance.CurBalance + (expenseReimAmt - empCashOnHand);
                                }
                                else //all other scenarios
                                {
                                    disbAndClaimItem.AmountToCredit = expenseReimAmt;
                                    disbAndClaimItem.AmountToWallet = 0;

                                }



                                disbAndClaimItem.ApprovalStatusId = (int)EApprovalStatus.Approved;

                                empCurrentCashAdvanceBalance.UpdatedOn = DateTime.UtcNow;
                                //_context.EmpCurrentPettyCashBalances.Update(empCurrentPettyCashBalance);
                                _context.Update(disbAndClaimItem);


                                //Final Approveer hence update the EmpCurrentPettyCashBalance table for the employee to reflect the credit
                                //empCurrentPettyCashBalance.CurBalance = empCurPettyBal + disbAndClaimItem.AmountToWallet ?? 0;
                                //empCurrentPettyCashBalance.UpdatedOn = DateTime.UtcNow;
                                //_context.EmpCurrentPettyCashBalances.Update(empCurrentPettyCashBalance);

                                ///
                            }

                            //Save to database
                            if (claimitem != null) { _context.Update(claimitem); };
                            await _context.SaveChangesAsync();

                            
                            var getEmpClaimApproversAllLevels = _context.ApprovalRoleMaps.Include(a => a.ApprovalLevel).Where(a => a.ApprovalGroupId == apprGroupId).OrderBy(o => o.ApprovalLevel.Level).ToList();


                            foreach (var ApprMap in getEmpClaimApproversAllLevels)
                            {

                                //only next level (level + 1) approver is considered here
                                if (ApprMap.ApprovalLevelId == expenseReimburseStatusTracker.ApprovalLevelId + 1)
                                {
                                    int? jobRoleid = ApprMap.JobRoleId;
                                    var apprEmpId = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == jobRoleid && e.ApprovalGroupId == apprGroupId).FirstOrDefault().EmployeeId;

                                    var approver = await _context.Employees.FindAsync(apprEmpId);

                                    //##### 4. Send email to the Approver
                                    //####################################
                                    _logger.LogInformation("Sending email to Approver " + approver.GetFullName());

                                    string[] paths = { Directory.GetCurrentDirectory(), "EmailTemplate", "ExpApprNotificationEmail.html" };
                                    string FilePath = Path.Combine(paths);
                                    _logger.LogInformation("Email template path " + FilePath);
                                    StreamReader str = new StreamReader(FilePath);
                                    string MailText = str.ReadToEnd();
                                    str.Close();

                                    var expReimReqt = _context.ExpenseReimburseRequests.Find(expenseReimburseStatusTracker.ExpenseReimburseRequestId);
                                    var approverMailAddress = approver.Email;
                                    string subject = expReimReqt.ExpenseReportTitle + " - #" + expenseReimburseStatusTracker.ExpenseReimburseRequest.Id.ToString();
                                    Employee emp = _context.Employees.Find(expenseReimburseStatusTracker.EmployeeId);


                                    var builder = new MimeKit.BodyBuilder();

                                    MailText = MailText.Replace("{Requester}", emp.GetFullName());
                                    MailText = MailText.Replace("{ApproverName}", approver.GetFullName());
                                    MailText = MailText.Replace("{Currency}", _context.CurrencyTypes.Find(emp.CurrencyTypeId).CurrencyCode);
                                    MailText = MailText.Replace("{RequestedAmount}", expenseReimburseStatusTracker.TotalClaimAmount.ToString());
                                    MailText = MailText.Replace("{RequestNumber}", qExpReimRequestId.ToString());
                                    builder.HtmlBody = MailText;

                                    var messagemail = new Message(new string[] { approverMailAddress }, subject, builder.HtmlBody);

                                    await _emailSender.SendEmailAsync(messagemail);
                                    _logger.LogInformation("Email sent to " + approver.GetFullName());

                                    break;


                                }
                            }
                        }

                        //if nothing else then just update the approval status
                        expenseReimburseStatusTracker.ApprovalStatusTypeId = expenseReimburseStatusTrackerDto.ApprovalStatusTypeId;

                        _context.ExpenseReimburseStatusTrackers.Update(expenseReimburseStatusTracker);

                        //If no expenseReimburseStatusTrackers are in pending for the Expense request then update the ExpenseReimburse request table

                        int pendingApprovals = _context.ExpenseReimburseStatusTrackers
                                  .Where(t => t.ExpenseReimburseRequestId == expenseReimburseStatusTrackerDto.ExpenseReimburseRequestId &&
                                  t.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).Count();

                        if (pendingApprovals == 0)
                        {
                            var expReimbReq = _context.ExpenseReimburseRequests.Where(p => p.Id == expenseReimburseStatusTrackerDto.ExpenseReimburseRequestId).FirstOrDefault();
                            expReimbReq.ApprovalStatusTypeId = expenseReimburseStatusTrackerDto.ApprovalStatusTypeId;
                            expReimbReq.ApproverActionDate = DateTime.UtcNow;
                            expReimbReq.Comments = bRejectMessage ? expenseReimburseStatusTrackerDto.Comments : "Approved";
                            _context.ExpenseReimburseRequests.Update(expReimbReq);
                            await _context.SaveChangesAsync();
                        }



                        //update the Expense Reimburse request table to reflect the rejection
                        if (bRejectMessage)
                        {
                            var expReimbReq = _context.ExpenseReimburseRequests.Where(p => p.Id == expenseReimburseStatusTrackerDto.ExpenseReimburseRequestId).FirstOrDefault();
                            expReimbReq.ApprovalStatusTypeId = expenseReimburseStatusTrackerDto.ApprovalStatusTypeId;
                            expReimbReq.ApproverActionDate = DateTime.UtcNow;
                            expReimbReq.Comments = expenseReimburseStatusTrackerDto.Comments;
                            _context.ExpenseReimburseRequests.Update(expReimbReq);

                            //DisbursementAndClaimsMaster update the record to Rejected (ApprovalStatusId = 5)
                            int disbAndClaimItemId = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == expReimbReq.Id).FirstOrDefault().Id;
                            var disbAndClaimItem = await _context.DisbursementsAndClaimsMasters.FindAsync(disbAndClaimItemId);

                            disbAndClaimItem.ApprovalStatusId = (int)EApprovalStatus.Rejected;
                            _context.Update(disbAndClaimItem);

                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "disbAndClaimItem update failed");
                                throw;
                            }


                        }

                    }
                    _context.ExpenseReimburseStatusTrackers.Update(expenseReimburseStatusTracker);


                }
                try
                {

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PutExpsensReimburseStatus Tracker record updation failed");
                }
                _logger.LogInformation("PutExpsensReimburseStatus Tracker record updation success");
                await _context.SaveChangesAsync();
                await AtoCashDbContextTransaction.CommitAsync();
            }


            RespStatus respStatus = new();

            if (bRejectMessage)
            {
                respStatus.Status = "Success";
                respStatus.Message = "Expense-Reimburse Request(s) Rejected!";
            }
            else
            {
                respStatus.Status = "Success";
                respStatus.Message = "Expense-Reimburse Request(s) Approved!";
            }

            return Ok(respStatus);

        }

        // POST: api/ExpenseReimburseStatusTrackers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ExpenseReimburseStatusTracker>> PostExpenseReimburseStatusTracker(ExpenseReimburseStatusTracker expenseReimburseStatusTracker)
        {
            _context.ExpenseReimburseStatusTrackers.Add(expenseReimburseStatusTracker);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExpenseReimburseStatusTracker", new { id = expenseReimburseStatusTracker.Id }, expenseReimburseStatusTracker);
        }

        // DELETE: api/ExpenseReimburseStatusTrackers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpenseReimburseStatusTracker(int id)
        {
            var expenseReimburseStatusTracker = await _context.ExpenseReimburseStatusTrackers.FindAsync(id);


            if (expenseReimburseStatusTracker == null)
            {
                _logger.LogError("Expense Tracker Id is invalid.. cant delete");
                return Conflict(new RespStatus { Status = "Failure", Message = "Expense Reimburse Request Id is Invalid!" });
            }


            _context.ExpenseReimburseStatusTrackers.Remove(expenseReimburseStatusTracker);
            await _context.SaveChangesAsync();

            return Ok(new RespStatus { Status = "Success", Message = "Expense Reimburse Request Deleted!" });
        }

        private bool ExpenseReimburseStatusTrackerExists(int id)
        {
            return _context.ExpenseReimburseStatusTrackers.Any(e => e.Id == id);
        }
    }
}
