using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtoCashAPI.Data;
using AtoCashAPI.Models;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using EmailService;
using Microsoft.AspNetCore.Authorization;
using AtoCashAPI.Authentication;
using System.Net.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace AtoCashAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr, User")]
    public class ExpenseReimburseRequestsController : ControllerBase
    {
        private readonly AtoCashDbContext _context;
        //private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExpenseReimburseRequestsController> _logger;

        public ExpenseReimburseRequestsController(AtoCashDbContext context,
                                                IWebHostEnvironment hostEnv,
                                                IEmailSender emailSender,
                                                ILogger<ExpenseReimburseRequestsController> logger)
        {
            _context = context;
            hostingEnvironment = hostEnv;
            _emailSender = emailSender;
            _logger = logger;
        }

        // GET: api/ExpenseReimburseRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseReimburseRequestDTO>>> GetExpenseReimburseRequests()
        {

            var expenseReimburseRequests = await _context.ExpenseReimburseRequests.ToListAsync();


            List<ExpenseReimburseRequestDTO> ListExpenseReimburseRequestDTO = new();


            foreach (ExpenseReimburseRequest expenseReimbRequest in expenseReimburseRequests)
            {

                var disbAndClaim = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == expenseReimbRequest.Id).FirstOrDefault();

                if (disbAndClaim == null)
                {
                    _logger.LogError("Disbursement table is empty for " + expenseReimbRequest.Id);
                }

                try
                {
                    ExpenseReimburseRequestDTO expenseReimburseRequestDTO = new();

                    expenseReimburseRequestDTO.Id = expenseReimbRequest.Id;
                    expenseReimburseRequestDTO.EmployeeId = expenseReimbRequest.EmployeeId;
                    expenseReimburseRequestDTO.EmployeeName = _context.Employees.Find(expenseReimbRequest.EmployeeId).GetFullName();
                    expenseReimburseRequestDTO.ExpenseReportTitle = expenseReimbRequest.ExpenseReportTitle;
                    expenseReimburseRequestDTO.CurrencyTypeId = expenseReimbRequest.CurrencyTypeId;
                    expenseReimburseRequestDTO.TotalClaimAmount = expenseReimbRequest.TotalClaimAmount;

                    expenseReimburseRequestDTO.BusinessTypeId = expenseReimbRequest.BusinessTypeId;
                    expenseReimburseRequestDTO.BusinessType = expenseReimbRequest.BusinessTypeId != null ? _context.BusinessTypes.Find(expenseReimbRequest.BusinessTypeId).BusinessTypeName : null;
                    expenseReimburseRequestDTO.BusinessUnitId = expenseReimbRequest.BusinessUnitId;
                    expenseReimburseRequestDTO.BusinessUnit = expenseReimbRequest.BusinessUnitId != null ? _context.BusinessUnits.Find(expenseReimbRequest.BusinessUnitId).GetBusinessUnitName() : null;
                    expenseReimburseRequestDTO.CostCentre = expenseReimbRequest.CostCenterId != null ? _context.CostCenters.Find(expenseReimbRequest.CostCenterId).GetCostCentre() : null;
                    expenseReimburseRequestDTO.ProjectId = expenseReimbRequest.ProjectId;
                    expenseReimburseRequestDTO.ProjectName = expenseReimbRequest.ProjectId != null ? _context.Projects.Find(expenseReimbRequest.ProjectId).ProjectName : null;


                    if (expenseReimbRequest.BusinessUnitId != null)
                    {
                        var locationId = _context.BusinessUnits.Find(expenseReimbRequest.BusinessUnitId).LocationId;
                        expenseReimburseRequestDTO.Location = _context.Locations.Find(locationId).LocationName;
                    }

                    expenseReimburseRequestDTO.SubProjectId = expenseReimbRequest.SubProjectId;
                    expenseReimburseRequestDTO.SubProjectName = expenseReimbRequest.SubProjectId != null ? _context.SubProjects.Find(expenseReimbRequest.SubProjectId).SubProjectName : null;

                    expenseReimburseRequestDTO.WorkTaskId = expenseReimbRequest.WorkTaskId;
                    expenseReimburseRequestDTO.WorkTaskName = expenseReimbRequest.WorkTaskId != null ? _context.WorkTasks.Find(expenseReimbRequest.WorkTaskId).TaskName : null;

                    expenseReimburseRequestDTO.RequestDate = expenseReimbRequest.RequestDate;
                    expenseReimburseRequestDTO.ApproverActionDate = expenseReimbRequest.ApproverActionDate;
                    expenseReimburseRequestDTO.ApprovalStatusTypeId = expenseReimbRequest.ApprovalStatusTypeId;
                    expenseReimburseRequestDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(expenseReimbRequest.ApprovalStatusTypeId).Status;

                    expenseReimburseRequestDTO.Comments = expenseReimbRequest.Comments;
                    expenseReimburseRequestDTO.CreditToBank = disbAndClaim.IsSettledAmountCredited ?? false ? disbAndClaim.AmountToCredit : 0;
                    expenseReimburseRequestDTO.CreditToWallet = disbAndClaim.IsSettledAmountCredited ?? false ? disbAndClaim.AmountToWallet : 0;
                    expenseReimburseRequestDTO.IsSettled = !(disbAndClaim.IsSettledAmountCredited ?? false);


                    ListExpenseReimburseRequestDTO.Add(expenseReimburseRequestDTO);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetExpenseReimburseRequests DTO Error expenseReimbRequest.Id " + expenseReimbRequest.Id);
                }


            }

            return ListExpenseReimburseRequestDTO.OrderByDescending(o => o.RequestDate).ToList();
        }

        //GET: api/ExpenseReimburseRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseReimburseRequestDTO>> GetExpenseReimburseRequest(int id)
        {


            var expenseReimbRequest = await _context.ExpenseReimburseRequests.FindAsync(id);

            if (expenseReimbRequest == null)
            {
                _logger.LogError("Expense Reimburse request is null for GetExpenseReimburseRequest " + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Expense Reimburse Id invalid!" });
            }


            double tmpAmountToCredit = 0;
            double tmpAmountToWallet = 0;
            bool tmpIsSettledAmountCredited = false;

            var disbAndClaim = _context.DisbursementsAndClaimsMasters.Where(d => d.BlendedRequestId == id).FirstOrDefault();



            if (disbAndClaim != null)
            {
                tmpAmountToCredit = disbAndClaim.AmountToCredit ?? 0;
                tmpAmountToWallet = disbAndClaim.AmountToWallet ?? 0;
                tmpIsSettledAmountCredited = (bool)disbAndClaim.IsSettledAmountCredited;

            }


            ExpenseReimburseRequestDTO expenseReimburseRequestDTO = new();


            try
            {



                expenseReimburseRequestDTO.Id = expenseReimbRequest.Id;
                expenseReimburseRequestDTO.EmployeeId = expenseReimbRequest.EmployeeId;
                expenseReimburseRequestDTO.EmployeeName = _context.Employees.Find(expenseReimbRequest.EmployeeId).GetFullName();
                expenseReimburseRequestDTO.ExpenseReportTitle = expenseReimbRequest.ExpenseReportTitle;
                expenseReimburseRequestDTO.CurrencyTypeId = expenseReimbRequest.CurrencyTypeId;
                expenseReimburseRequestDTO.TotalClaimAmount = expenseReimbRequest.TotalClaimAmount;

                expenseReimburseRequestDTO.BusinessTypeId = expenseReimbRequest.BusinessTypeId;
                expenseReimburseRequestDTO.BusinessType = expenseReimbRequest.BusinessTypeId != null ? _context.BusinessTypes.Find(expenseReimbRequest.BusinessTypeId).BusinessTypeName : null;
                expenseReimburseRequestDTO.BusinessUnitId = expenseReimbRequest.BusinessUnitId;
                expenseReimburseRequestDTO.BusinessUnit = expenseReimbRequest.BusinessUnitId != null ? _context.BusinessUnits.Find(expenseReimbRequest.BusinessUnitId).GetBusinessUnitName() : null;
                
                if (expenseReimbRequest.BusinessUnitId != null)
                {
                    var locationId = _context.BusinessUnits.Find(expenseReimbRequest.BusinessUnitId).LocationId;
                    expenseReimburseRequestDTO.Location = _context.Locations.Find(locationId).LocationName;
                }

                expenseReimburseRequestDTO.CostCentre = expenseReimbRequest.CostCenterId != null ? _context.CostCenters.Find(expenseReimbRequest.CostCenterId).GetCostCentre() : null;
                expenseReimburseRequestDTO.ProjectId = expenseReimbRequest.ProjectId;
                expenseReimburseRequestDTO.ProjectName = expenseReimbRequest.ProjectId != null ? _context.Projects.Find(expenseReimbRequest.ProjectId).ProjectName : null;

                expenseReimburseRequestDTO.SubProjectId = expenseReimbRequest.SubProjectId;
                expenseReimburseRequestDTO.SubProjectName = expenseReimbRequest.SubProjectId != null ? _context.SubProjects.Find(expenseReimbRequest.SubProjectId).SubProjectName : null;

                expenseReimburseRequestDTO.WorkTaskId = expenseReimbRequest.WorkTaskId;
                expenseReimburseRequestDTO.WorkTaskName = expenseReimbRequest.WorkTaskId != null ? _context.WorkTasks.Find(expenseReimbRequest.WorkTaskId).TaskName : null;

                expenseReimburseRequestDTO.RequestDate = expenseReimbRequest.RequestDate;
                expenseReimburseRequestDTO.ApproverActionDate = expenseReimbRequest.ApproverActionDate;
                expenseReimburseRequestDTO.ApprovalStatusTypeId = expenseReimbRequest.ApprovalStatusTypeId;
                expenseReimburseRequestDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(expenseReimbRequest.ApprovalStatusTypeId).Status;

                expenseReimburseRequestDTO.Comments = expenseReimbRequest.Comments;
                expenseReimburseRequestDTO.CreditToBank = disbAndClaim.IsSettledAmountCredited ?? false ? disbAndClaim.AmountToCredit : 0;
                expenseReimburseRequestDTO.CreditToWallet = disbAndClaim.IsSettledAmountCredited ?? false ? disbAndClaim.AmountToWallet : 0;
                expenseReimburseRequestDTO.IsSettled = !(disbAndClaim.IsSettledAmountCredited ?? false);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetExpenseReimburseRequest failed for " + id);
            }


            return expenseReimburseRequestDTO;

        }



        [HttpGet("{id}")]
        [ActionName("GetExpenseReimburseRequestRaisedForEmployee")]
        public async Task<ActionResult<IEnumerable<ExpenseReimburseRequestDTO>>> GetExpenseReimburseRequestRaisedForEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            var ListEmpExtInfos = await _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == id).ToListAsync();

            if (employee == null)
            {
                _logger.LogError("Travel : Employee Id is not valid:" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id is Invalid!" });
            }

            var expenseReimbRequests = await _context.ExpenseReimburseRequests.Where(p => p.EmployeeId == id).ToListAsync();

            if (expenseReimbRequests == null)
            {
                _logger.LogError("No ExpenseReimburse Requests raised" + id);
                return Conflict(new RespStatus { Status = "Failure", Message = "No ExpenseReimburse Requests raised!" });
            }

            List<ExpenseReimburseRequestDTO> ListExpenseReimburseRequestDTO = new();

            foreach (ExpenseReimburseRequest expenseReimbRequest in expenseReimbRequests)
            {
                ExpenseReimburseRequestDTO expenseReimburseRequestDTO = new();



                expenseReimburseRequestDTO.Id = expenseReimbRequest.Id;
                expenseReimburseRequestDTO.EmployeeId = expenseReimbRequest.EmployeeId;
                expenseReimburseRequestDTO.EmployeeName = _context.Employees.Find(expenseReimbRequest.EmployeeId).GetFullName();
                expenseReimburseRequestDTO.ExpenseReportTitle = expenseReimbRequest.ExpenseReportTitle;
                expenseReimburseRequestDTO.CurrencyTypeId = expenseReimbRequest.CurrencyTypeId;
                expenseReimburseRequestDTO.TotalClaimAmount = expenseReimbRequest.TotalClaimAmount;

                expenseReimburseRequestDTO.BusinessTypeId = expenseReimbRequest.BusinessTypeId;
                expenseReimburseRequestDTO.BusinessType = expenseReimbRequest.BusinessTypeId != null ? _context.BusinessTypes.Find(expenseReimbRequest.BusinessTypeId).BusinessTypeName : null;
                expenseReimburseRequestDTO.BusinessUnitId = expenseReimbRequest.BusinessUnitId;
                expenseReimburseRequestDTO.BusinessUnit = expenseReimbRequest.BusinessUnitId != null ? _context.BusinessUnits.Find(expenseReimbRequest.BusinessUnitId).GetBusinessUnitName() : null;
                expenseReimburseRequestDTO.CostCentre = expenseReimbRequest.CostCenterId != null ? _context.CostCenters.Find(expenseReimbRequest.CostCenterId).GetCostCentre() : null;
                expenseReimburseRequestDTO.ProjectId = expenseReimbRequest.ProjectId;
                expenseReimburseRequestDTO.ProjectName = expenseReimbRequest.ProjectId != null ? _context.Projects.Find(expenseReimbRequest.ProjectId).ProjectName : null;

                expenseReimburseRequestDTO.SubProjectId = expenseReimbRequest.SubProjectId;
                expenseReimburseRequestDTO.SubProjectName = expenseReimbRequest.SubProjectId != null ? _context.SubProjects.Find(expenseReimbRequest.SubProjectId).SubProjectName : null;

                expenseReimburseRequestDTO.WorkTaskId = expenseReimbRequest.WorkTaskId;
                expenseReimburseRequestDTO.WorkTaskName = expenseReimbRequest.WorkTaskId != null ? _context.WorkTasks.Find(expenseReimbRequest.WorkTaskId).TaskName : null;

                if (expenseReimbRequest.BusinessUnitId != null)
                {
                    var locationId = _context.BusinessUnits.Find(expenseReimbRequest.BusinessUnitId).LocationId;
                    expenseReimburseRequestDTO.Location = _context.Locations.Find(locationId).LocationName;
                }

                expenseReimburseRequestDTO.RequestDate = expenseReimbRequest.RequestDate;
                expenseReimburseRequestDTO.ApproverActionDate = expenseReimbRequest.ApproverActionDate;
                expenseReimburseRequestDTO.ApprovalStatusTypeId = expenseReimbRequest.ApprovalStatusTypeId;
                expenseReimburseRequestDTO.ApprovalStatusType = _context.ApprovalStatusTypes.Find(expenseReimbRequest.ApprovalStatusTypeId).Status;

                expenseReimburseRequestDTO.Comments = expenseReimbRequest.Comments;

                // set the bookean flat to TRUE if No approver has yet approved the Request else FALSE
                bool ifAnyOfStatusRecordsApproved = _context.ExpenseReimburseStatusTrackers.Where(t =>
                                                         (t.ApprovalStatusTypeId == (int)EApprovalStatus.Rejected ||
                                                          t.ApprovalStatusTypeId == (int)EApprovalStatus.Approved) &&
                                                          t.ExpenseReimburseRequestId == expenseReimbRequest.Id).Any();

                expenseReimburseRequestDTO.ShowEditDelete = ifAnyOfStatusRecordsApproved ? false : true;

                ListExpenseReimburseRequestDTO.Add(expenseReimburseRequestDTO);

            }



            return Ok(ListExpenseReimburseRequestDTO.OrderByDescending(o => o.RequestDate).ToList());
        }



        [HttpGet("{id}")]
        [ActionName("CountAllBusinessAreaExpenseReimburseRequestRaisedByEmployee")]
        public async Task<ActionResult> CountAllBusinessAreaExpenseReimburseRequestRaisedByEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                _logger.LogError("Employee invalid Id:" + id);
                return Ok(0);
            }

            var expenseReimburseRequests = await _context.ExpenseReimburseRequests.Where(p => p.EmployeeId == id).ToListAsync();

            if (expenseReimburseRequests == null)
            {
                _logger.LogInformation("BusinessAreaExpenseReimburseRequests is null with Business Area request for Employeed Id" + id);
                return Ok(0);
            }

            int TotalCount = _context.ExpenseReimburseRequests.Where(c => c.EmployeeId == id).Count();
            int PendingCount = _context.ExpenseReimburseRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).Count();
            int RejectedCount = _context.ExpenseReimburseRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Rejected).Count();
            int ApprovedCount = _context.ExpenseReimburseRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Approved).Count();

            return Ok(new { TotalCount, PendingCount, RejectedCount, ApprovedCount });
        }


        [HttpGet("{id}")]
        [ActionName("CountAllExpenseReimburseRequestRaisedByEmployee")]
        public async Task<ActionResult> CountAllExpenseReimburseRequestRaisedByEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                _logger.LogError("Employee invalid Id:" + id);
                return Ok(0);
            }

            var expenseReimburseRequests = await _context.ExpenseReimburseRequests.Where(p => p.EmployeeId == id).ToListAsync();

            if (expenseReimburseRequests == null)
            {
                _logger.LogInformation("BusinessAreaExpenseReimburseRequests is null with Business Area request for Employeed Id" + id);
                return Ok(0);
            }

            int TotalCount = _context.ExpenseReimburseRequests.Where(c => c.EmployeeId == id).Count();
            int PendingCount = _context.ExpenseReimburseRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Pending).Count();
            int RejectedCount = _context.ExpenseReimburseRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Rejected).Count();
            int ApprovedCount = _context.ExpenseReimburseRequests.Where(c => c.EmployeeId == id && c.ApprovalStatusTypeId == (int)EApprovalStatus.Approved).Count();

            return Ok(new { TotalCount, PendingCount, RejectedCount, ApprovedCount });
        }


        // PUT: api/ExpenseReimburseRequests/5
        [HttpPut]
        //[Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> PutExpenseReimburseRequest(int id, ExpenseReimburseRequestDTO expenseReimbRequestDTO)
        {
            if (id != expenseReimbRequestDTO.Id)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Id is invalid" });
            }


            try
            {
                var expenseReimbRequest = await _context.ExpenseReimburseRequests.FindAsync(expenseReimbRequestDTO.Id);

                int? reqBussUnitId = expenseReimbRequestDTO.BusinessUnitId;
                int costCenterId = _context.BusinessUnits.Find(reqBussUnitId).CostCenterId ?? 0;


                // expenseReimbRequest.Id = expenseReimbRequestDTO.Id;
                expenseReimbRequest.ExpenseReportTitle = expenseReimbRequestDTO.ExpenseReportTitle;
                expenseReimbRequest.BusinessTypeId = expenseReimbRequestDTO.BusinessTypeId;
                expenseReimbRequest.BusinessUnitId = expenseReimbRequestDTO.BusinessUnitId;
                expenseReimbRequest.EmployeeId = expenseReimbRequestDTO.EmployeeId;
                expenseReimbRequest.CurrencyTypeId = expenseReimbRequestDTO.CurrencyTypeId;
                expenseReimbRequest.TotalClaimAmount = expenseReimbRequestDTO.TotalClaimAmount; //Initially Zero but will be updated after all subclaimes added as per the request
                expenseReimbRequest.RequestDate = DateTime.UtcNow;
                expenseReimbRequest.CostCenterId = costCenterId;
                expenseReimbRequest.ProjectId = expenseReimbRequestDTO.ProjectId;
                expenseReimbRequest.SubProjectId = expenseReimbRequestDTO.SubProjectId;
                expenseReimbRequest.WorkTaskId = expenseReimbRequestDTO.WorkTaskId;
                expenseReimbRequest.ApproverActionDate = expenseReimbRequestDTO.ApproverActionDate;
                expenseReimbRequest.ApprovalStatusTypeId = expenseReimbRequestDTO.ApprovalStatusTypeId;
                expenseReimbRequest.Comments = "Expense Reimburse Request in Process!";


                await Task.Run(() => _context.ExpenseReimburseRequests.Update(expenseReimbRequest));


                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "DbUpdateConcurrencyException PutExpenseReimburseRequest " + id);
            }

            return Ok(new RespStatus { Status = "Success", Message = "Expense Reimburse Data Updated!" });
        }

        // POST: api/ExpenseReimburseRequests

        [HttpPost]
        [ActionName("PostDocuments")]
        public async Task<ActionResult<List<FileDocumentDTO>>> PostFiles([FromForm] IFormFileCollection Documents)
        {
            //StringBuilder StrBuilderUploadedDocuments = new();

            List<FileDocumentDTO> fileDocumentDTOs = new();


            foreach (IFormFile document in Documents)
            {
                //Store the file to the contentrootpath/images =>
                //for docker it is /app/Images configured with volume mount in docker-compose

                string uploadsFolder = Path.Combine(hostingEnvironment.ContentRootPath, "Images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + document.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);


                try
                {
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await document.CopyToAsync(stream);
                    stream.Flush();


                    // Save it to the acutal FileDocuments table
                    FileDocument fileDocument = new();
                    fileDocument.ActualFileName = document.FileName;
                    fileDocument.UniqueFileName = uniqueFileName;
                    _context.FileDocuments.Add(fileDocument);
                    await _context.SaveChangesAsync();
                    //

                    // Populating the List of Document Id for FrontEnd consumption
                    FileDocumentDTO fileDocumentDTO = new();
                    fileDocumentDTO.Id = fileDocument.Id;
                    fileDocumentDTO.ActualFileName = document.FileName;
                    fileDocumentDTOs.Add(fileDocumentDTO);

                    //StrBuilderUploadedDocuments.Append(uniqueFileName + "^");
                    //
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Post IformFile documents");
                    return Conflict(new RespStatus { Status = "Failure", Message = "File not uploaded.. Please retry!" + ex.ToString() });

                }




            }

            return Ok(fileDocumentDTOs);
        }

        //############################################################################################################
        /// <summary>
        /// Dont delete the below code code
        /// </summary>
        //############################################################################################################

        ///
        //[HttpGet("{id}")]
        //[ActionName("GetDocumentsBySubClaimsId")]
        ////<List<FileContentResult>
        //public async Task<ActionResult> GetDocumentsBySubClaimsId(int id)
        //{
        //    List<string> documentIds = _context.ExpenseSubClaims.Find(id).DocumentIDs.Split(",").ToList();
        //    string documentsFolder = Path.Combine(hostingEnvironment.ContentRootPath, "Images");
        //    //var content = new MultipartContent();

        //    List<FileContentResult> ListOfDocuments = new();
        //    var provider = new FileExtensionContentTypeProvider();

        //    foreach (string doc in documentIds)
        //    {
        //        var fd = _context.FileDocuments.Find(id);
        //        string uniqueFileName = fd.UniqueFileName;
        //        string actualFileName = fd.ActualFileName;

        //        string filePath = Path.Combine(documentsFolder, uniqueFileName);
        //        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        //        if (!provider.TryGetContentType(filePath, out var contentType))
        //        {
        //            contentType = "application/octet-stream";
        //        }

        //        FileContentResult thisfile = File(bytes, contentType, Path.GetFileName(filePath));

        //        ListOfDocuments.Add(thisfile);
        //    }
        //    return Ok(ListOfDocuments);
        //}
        //############################################################################################################

        [HttpGet("{id}")]
        [ActionName("GetDocumentsBySubClaimsId")]
        //<List<FileContentResult>
        public async Task<ActionResult> GetDocumentsBySubClaimsId(int id)
        {
            List<int> documentIds = _context.ExpenseSubClaims.Find(id).DocumentIDs.Split(",").Select(Int32.Parse).ToList();
            string documentsFolder = Path.Combine(hostingEnvironment.ContentRootPath, "Images");

            List<string> docUrls = new();

            var provider = new FileExtensionContentTypeProvider();
            await Task.Run(() =>
            {
                foreach (int docid in documentIds)
                {
                    var fd = _context.FileDocuments.Find(docid);
                    string uniqueFileName = fd.UniqueFileName;
                    string actualFileName = fd.ActualFileName;

                    string filePath = Path.Combine(documentsFolder, uniqueFileName);

                    string docUrl = Directory.EnumerateFiles(documentsFolder).Select(f => filePath).FirstOrDefault().ToString();
                    docUrls.Add(docUrl);


                }
            });
            _logger.LogInformation("GetDocumentsBySubClaimsId - retrieved documents successfully");
            return Ok(docUrls);
        }


        [HttpGet("{id}")]
        [ActionName("GetDocumentByDocId")]
        public async Task<ActionResult> GetDocumentByDocId(int id)
        {
            string documentsFolder = Path.Combine(hostingEnvironment.ContentRootPath, "Images");
            //var content = new MultipartContent();

            var provider = new FileExtensionContentTypeProvider();

            var fd = _context.FileDocuments.Find(id);
            string uniqueFileName = fd.UniqueFileName;
            //string actualFileName = fd.ActualFileName;

            string filePath = Path.Combine(documentsFolder, uniqueFileName);
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            //FileContentResult thisfile = File(bytes, contentType, Path.GetFileName(filePath));

            _logger.LogInformation("GetDocumentByDocId - returned document to caller");

            return File(bytes, contentType, Path.GetFileName(filePath));
        }



        [HttpPost]

        //[ActionName("PostExpenseReimburseRequest")]
        public async Task<ActionResult> PostExpenseReimburseRequest(ExpenseReimburseRequestDTO expenseReimburseRequestDto)
        {
            int SuccessResult;

            if (expenseReimburseRequestDto == null)
            {
                _logger.LogError("PostExpenseReimburseRequest - null request data");
                return Conflict(new RespStatus { Status = "Failure", Message = "expenseReimburse Request invalid!" });
            }

            if (expenseReimburseRequestDto.ProjectId != null)
            {
                //Goes to Option 1 (Project)
                SuccessResult = await Task.Run(() => ProjectBasedExpReimRequest(expenseReimburseRequestDto));
            }
            else
            {
                //Goes to Option 3 (Business Unit)
                SuccessResult = await Task.Run(() => BusinessUnitBasedExpReimRequest(expenseReimburseRequestDto));
            }

            if (SuccessResult == 0)
            {
                _logger.LogInformation("PostExpenseReimburseRequest - Process completed");

                return Ok(new RespStatus { Status = "Success", Message = "Expense Reimburse Request Created!" });
            }
            else
            {
                _logger.LogError("Expense Reimburse Request creation failed!");

                return BadRequest(new RespStatus { Status = "Failure", Message = "Expense Reimburse Request creation failed!" });
            }

        }


        // DELETE: api/ExpenseReimburseRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpenseReimburseRequest(int id)
        {
            var expenseReimburseRequest = await _context.ExpenseReimburseRequests.FindAsync(id);
            if (expenseReimburseRequest == null)
            {
                _logger.LogError("expenseReimburseRequest - request data null");

                return Conflict(new RespStatus { Status = "Failure", Message = "expense Reimburse Request Id Invalid!" });
            }

            int ApprovedCount = _context.ExpenseReimburseStatusTrackers.Where(e => e.ExpenseReimburseRequestId == expenseReimburseRequest.Id && e.ApprovalStatusTypeId == (int)EApprovalStatus.Approved).Count();

            if (ApprovedCount != 0)
            {
                _logger.LogInformation("expenseReimburseRequest - Reimburse Request cant be Deleted after Approval");
                return Conflict(new RespStatus { Status = "Failure", Message = "Reimburse Request cant be Deleted after Approval!" });
            }


            _context.ExpenseReimburseRequests.Remove(expenseReimburseRequest);

            await _context.SaveChangesAsync();
            _logger.LogInformation("expenseReimburseRequest - Deleted");
            return Ok(new RespStatus { Status = "Success", Message = "Expense Reimburse Request Deleted!" });
        }



        /// <summary>
        /// Business Unit based Expreimburse request
        /// </summary>
        /// <param name="expenseReimburseRequestDto"></param>
        /// <returns></returns>



        private async Task<int> BusinessUnitBasedExpReimRequest(ExpenseReimburseRequestDTO expenseReimburseRequestDto)
        {
            _logger.LogInformation("Business Unit Expense Reimburse Request Started");

            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {

                #region
                int? reqBussUnitId = expenseReimburseRequestDto.BusinessUnitId;
                int costCenterId = _context.BusinessUnits.Find(reqBussUnitId).CostCenterId ?? 0;
                int reqEmpid = expenseReimburseRequestDto.EmployeeId;
                bool isSelfApprovedRequest = false;
                double? dblTotalClaimAmount = 0;

                Employee reqEmp = _context.Employees.Find(reqEmpid);


                if (reqEmp == null)
                {
                    return 1;
                }
                EmployeeExtendedInfo reqEmpExtInfo = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == expenseReimburseRequestDto.EmployeeId && e.BusinessUnitId == reqBussUnitId).FirstOrDefault();

                int? reqJobRoleId = reqEmpExtInfo.JobRoleId;
                int? reqApprGroupId = reqEmpExtInfo.ApprovalGroupId;


                var approRoleMap = _context.ApprovalRoleMaps.Include("ApprovalLevel").Where(a => a.ApprovalGroupId == reqApprGroupId).FirstOrDefault();

                if (approRoleMap == null)
                {
                    _logger.LogError("Approver Role Map Not defined, approval group id " + reqApprGroupId);
                    return 1;
                }
                else
                {
                    var approRoleMaps = _context.ApprovalRoleMaps.Include("ApprovalLevel").Where(a => a.ApprovalGroupId == reqApprGroupId).ToList();

                    foreach (ApprovalRoleMap ApprMap in approRoleMaps)
                    {
                        int? jobRole_id = ApprMap.JobRoleId;

                        int? approverEmpId = _context.EmployeeExtendedInfos.Where(e => e.JobRoleId == jobRole_id && e.ApprovalGroupId == reqApprGroupId).FirstOrDefault().EmployeeId;

                        var approver = await _context.Employees.FirstAsync(e => e.Id == approverEmpId);
                        if (approver == null)
                        {
                            _logger.LogError("Approver employee not mapped for RoleMap RoleId:" + jobRole_id + "ApprovalGroupId:" + reqApprGroupId);
                            return 1;
                        }

                    }
                }

                int? maxApprLevel = _context.ApprovalRoleMaps.Include("ApprovalLevel").Where(a => a.ApprovalGroupId == reqApprGroupId).ToList().Select(x => x.ApprovalLevel).Max(a => a.Level);
                int? reqApprLevel = _context.ApprovalRoleMaps.Include("ApprovalLevel").Where(a => a.ApprovalGroupId == reqApprGroupId && a.JobRoleId == reqJobRoleId).Select(x => x.ApprovalLevel).FirstOrDefault().Level;


                _logger.LogInformation("All Approvers defined");

                ExpenseReimburseRequest expenseReimburseRequest = new();
                expenseReimburseRequest.ExpenseReportTitle = expenseReimburseRequestDto.ExpenseReportTitle;
                expenseReimburseRequest.BusinessTypeId = expenseReimburseRequestDto.BusinessTypeId;
                expenseReimburseRequest.BusinessUnitId = expenseReimburseRequestDto.BusinessUnitId;
                expenseReimburseRequest.EmployeeId = expenseReimburseRequestDto.EmployeeId;
                expenseReimburseRequest.CurrencyTypeId = expenseReimburseRequestDto.CurrencyTypeId;
                expenseReimburseRequest.TotalClaimAmount = dblTotalClaimAmount; //Initially Zero but will be updated after all subclaimes added as per the request
                expenseReimburseRequest.RequestDate = DateTime.UtcNow;
                expenseReimburseRequest.CostCenterId = costCenterId;
                expenseReimburseRequest.ProjectId = null;
                expenseReimburseRequest.SubProjectId = null;
                expenseReimburseRequest.WorkTaskId = null;
                expenseReimburseRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Pending;
                expenseReimburseRequest.ApproverActionDate = null;
                expenseReimburseRequest.Comments = "Expense Reimburse Request in Process!";
                _logger.LogInformation("Exp Reimb Table insert started");


                _context.ExpenseReimburseRequests.Add(expenseReimburseRequest); //  <= this generated the Id
                await _context.SaveChangesAsync();

                _logger.LogInformation("Expense Reimburse Table inserted successfully");

                //assign values

                _logger.LogInformation("Sub Claims section started");
                foreach (ExpenseSubClaimDTO expenseSubClaimDto in expenseReimburseRequestDto.ExpenseSubClaims)
                {
                    ExpenseSubClaim expenseSubClaim = new();

                    //get expensereimburserequestId from the saved record and then use here for sub-claims
                    expenseSubClaim.ExpenseReimburseRequestId = expenseReimburseRequest.Id;
                    expenseSubClaim.ExpenseTypeId = expenseSubClaimDto.ExpenseTypeId;
                    expenseSubClaim.EmployeeId = reqEmpid;
                    expenseSubClaim.ExpenseReimbClaimAmount = expenseSubClaimDto.ExpenseReimbClaimAmount;
                    expenseSubClaim.DocumentIDs = expenseSubClaimDto.DocumentIDs;
                    expenseSubClaim.InvoiceNo = expenseSubClaimDto.InvoiceNo;
                    expenseSubClaim.InvoiceDate = expenseSubClaimDto.InvoiceDate;
                    expenseSubClaim.ExpenseCategoryId = expenseSubClaimDto.ExpenseCategoryId;
                    expenseSubClaim.IsVAT = expenseSubClaimDto.IsVAT;
                    expenseSubClaim.TaxNo = expenseSubClaimDto.TaxNo;
                    expenseSubClaim.ExpStrtDate = expenseSubClaimDto.ExpStrtDate;
                    expenseSubClaim.ExpEndDate = expenseSubClaimDto.ExpEndDate;
                    expenseSubClaim.ExpNoOfDays = expenseSubClaimDto.ExpNoOfDays;
                    expenseSubClaim.RequestDate = DateTime.UtcNow;

                    expenseSubClaim.Tax = expenseSubClaimDto.Tax;
                    expenseSubClaim.TaxAmount = expenseSubClaimDto.TaxAmount;
                    expenseSubClaim.VendorId = expenseSubClaimDto.VendorId;
                    expenseSubClaim.AdditionalVendor = expenseSubClaimDto.VendorId == null ? expenseSubClaim.AdditionalVendor : String.Empty;
                    expenseSubClaim.Location = expenseSubClaimDto.Location;

                    expenseSubClaim.BusinessTypeId = expenseReimburseRequestDto.BusinessTypeId;
                    expenseSubClaim.BusinessUnitId = expenseReimburseRequestDto.BusinessUnitId;
                    expenseSubClaim.ProjectId = null;
                    expenseSubClaim.SubProjectId = null;
                    expenseSubClaim.WorkTaskId = null;
                    expenseSubClaim.CostCenterId = costCenterId;
                    expenseSubClaim.Description = expenseSubClaimDto.Description;
                    

                    _context.ExpenseSubClaims.Add(expenseSubClaim);

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Sub Claims Table insert failed");
                        return 1;
                    }

                    dblTotalClaimAmount = dblTotalClaimAmount + expenseSubClaimDto.TaxAmount + expenseSubClaimDto.ExpenseReimbClaimAmount;

                }

                ExpenseReimburseRequest exp = _context.ExpenseReimburseRequests.Find(expenseReimburseRequest.Id);

                exp.TotalClaimAmount = dblTotalClaimAmount;
                _context.ExpenseReimburseRequests.Update(exp);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TotalClaimAmount update failed");
                    return 1;
                }

                _logger.LogInformation("Sub Claims Table records inserted");


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
                    ExpenseReimburseStatusTracker expenseReimburseStatusTracker = new()
                    {
                        EmployeeId = reqEmpid,
                        CurrencyTypeId = expenseReimburseRequestDto.CurrencyTypeId,
                        TotalClaimAmount = dblTotalClaimAmount,
                        ExpenseReimburseRequestId = expenseReimburseRequest.Id,
                        BusinessTypeId = expenseReimburseRequestDto.BusinessTypeId,
                        BusinessUnitId = expenseReimburseRequestDto.BusinessUnitId,
                        ProjectId = null, //Approver Project Id
                        JobRoleId = reqJobRoleId,
                        ApprovalGroupId = reqApprGroupId,
                        ApprovalLevelId = reqApprLevel,
                        RequestDate = DateTime.UtcNow,
                        ApproverEmpId = reqEmpid,
                        ApproverActionDate = DateTime.UtcNow,
                        ApprovalStatusTypeId = (int)EApprovalStatus.Approved, //1-Pending, 2-Approved, 3-Rejected
                        Comments = "Requestor is highest approver level and is a Approver - Self Approved Request"
                    };
                    _context.ExpenseReimburseStatusTrackers.Add(expenseReimburseStatusTracker);
                    expenseReimburseRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                    expenseReimburseRequest.Comments = "Approved";
                    _context.ExpenseReimburseRequests.Update(expenseReimburseRequest);
                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("BusinessUnit: ExpenseReimburseRequest insert success");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Self approved ExpenseReimburseRequest update failed");
                        return 1;
                    }

                    _logger.LogInformation("Self Approved:Expense table Updated with Approved Status");
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

                        _logger.LogInformation(approver.GetFullName() + " Status Tracker started");
                        ExpenseReimburseStatusTracker expenseReimburseStatusTracker = new()
                        {
                            EmployeeId = reqEmpid,
                            CurrencyTypeId = expenseReimburseRequestDto.CurrencyTypeId,
                            TotalClaimAmount = dblTotalClaimAmount,
                            ExpenseReimburseRequestId = expenseReimburseRequest.Id,
                            BusinessTypeId = expenseReimburseRequestDto.BusinessTypeId,
                            BusinessUnitId = expenseReimburseRequestDto.BusinessUnitId,
                            ProjectId = null, //Approver Project Id
                            JobRoleId = apprjobRoleId,
                            ApprovalGroupId = reqApprGroupId,
                            ApprovalLevelId = ApprMap.ApprovalLevelId,
                            RequestDate = DateTime.UtcNow,
                            ApproverEmpId = null,
                            ApproverActionDate = DateTime.UtcNow,
                            ApprovalStatusTypeId = isFirstApprover ? (int)EApprovalStatus.Pending : (int)EApprovalStatus.Intitated,
                            Comments = "Awaiting Approver Action"
                        };
                        _context.ExpenseReimburseStatusTrackers.Add(expenseReimburseStatusTracker);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation(approver.GetFullName() + " Status Tracker inserted");
                        //##### 5. Send email to the Approver
                        //####################################

                        if (isFirstApprover)
                        {
                            _logger.LogInformation(approver.GetFullName() + "Email Start");

                            //  string[] paths = { Directory.GetCurrentDirectory(),
                            string[] paths = { Directory.GetCurrentDirectory(), "ExpApprNotificationEmail.html" };
                            string FilePath = Path.Combine(paths);
                            _logger.LogInformation("Email template path " + FilePath);
                            StreamReader str = new StreamReader(FilePath);
                            string MailText = str.ReadToEnd();
                            str.Close();

                            var approverMailAddress = approver.Email;
                            string subject = expenseReimburseRequest.ExpenseReportTitle + " - #" + expenseReimburseRequest.Id.ToString();
                            Employee emp = _context.Employees.Find(expenseReimburseRequestDto.EmployeeId);

                            var builder = new MimeKit.BodyBuilder();

                            MailText = MailText.Replace("{Requester}", emp.GetFullName());
                            MailText = MailText.Replace("{ApproverName}", approver.GetFullName());
                            MailText = MailText.Replace("{Currency}", _context.CurrencyTypes.Find(emp.CurrencyTypeId).CurrencyCode);
                            MailText = MailText.Replace("{RequestedAmount}", expenseReimburseRequest.TotalClaimAmount.ToString());
                            MailText = MailText.Replace("{RequestNumber}", expenseReimburseRequest.Id.ToString());
                            builder.HtmlBody = MailText;

                            var messagemail = new Message(new string[] { approverMailAddress }, subject, builder.HtmlBody);

                            await _emailSender.SendEmailAsync(messagemail);
                            _logger.LogInformation(approver.GetFullName() + "Email Sent");
                        }
                        isFirstApprover = false;

                        //repeat for each approver
                    }

                }

                //##### 5. Adding a entry in DisbursementsAndClaimsMaster table for records
                #region
                _logger.LogInformation("DisbursementsAndClaimsMaster table Start");

                DisbursementsAndClaimsMaster disbursementsAndClaimsMaster = new();
                disbursementsAndClaimsMaster.EmployeeId = reqEmpid;
                disbursementsAndClaimsMaster.BlendedRequestId = expenseReimburseRequest.Id;
                disbursementsAndClaimsMaster.RequestTypeId = (int)ERequestType.ExpenseReim;
                disbursementsAndClaimsMaster.BusinessTypeId = expenseReimburseRequestDto.BusinessTypeId;
                disbursementsAndClaimsMaster.BusinessUnitId = expenseReimburseRequestDto.BusinessUnitId;
                disbursementsAndClaimsMaster.ProjectId = null;
                disbursementsAndClaimsMaster.SubProjectId = null;
                disbursementsAndClaimsMaster.WorkTaskId = null;
                disbursementsAndClaimsMaster.RecordDate = DateTime.UtcNow;
                disbursementsAndClaimsMaster.CurrencyTypeId = expenseReimburseRequestDto.CurrencyTypeId;
                disbursementsAndClaimsMaster.ClaimAmount = dblTotalClaimAmount;
                disbursementsAndClaimsMaster.AmountToCredit = 0; //check this amount NOT zero
                disbursementsAndClaimsMaster.IsSettledAmountCredited = false;
                disbursementsAndClaimsMaster.CostCenterId = costCenterId;
                disbursementsAndClaimsMaster.ApprovalStatusId = isSelfApprovedRequest ? (int)EApprovalStatus.Approved : (int)EApprovalStatus.Pending;

                //save at the end of the code. not here!
                #endregion


                /// #############################
                //   Crediting back to the wallet (for self approvedRequest Only)
                /// #############################
                /// 

                if (isSelfApprovedRequest)
                {
                    var empExtInfo = _context.EmployeeExtendedInfos.Where(e => e.EmployeeId == reqEmpid && e.BusinessUnitId == expenseReimburseRequestDto.BusinessUnitId).FirstOrDefault();
                    double? expenseReimAmt = expenseReimburseRequest.TotalClaimAmount;
                    double? RoleLimitAmt = _context.JobRoles.Find(empExtInfo.JobRoleId).MaxCashAdvanceAllowed;

                    var empCurrentCashAdvanceBalance = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == reqEmp.Id).FirstOrDefault();
                    double? empCurCashAdvanceBal = empCurrentCashAdvanceBalance.CurBalance;

                    //logic goes here

                    if (expenseReimAmt + empCurCashAdvanceBal >= RoleLimitAmt) // claiming amount is greater than replishable amount
                    {
                        disbursementsAndClaimsMaster.AmountToWallet = RoleLimitAmt - empCurCashAdvanceBal;
                        disbursementsAndClaimsMaster.AmountToCredit = expenseReimAmt - (RoleLimitAmt - empCurCashAdvanceBal);

                    }
                    else
                    {
                        //fully credit to Wallet - Zero amount to bank amount
                        disbursementsAndClaimsMaster.AmountToWallet = expenseReimAmt;
                        disbursementsAndClaimsMaster.AmountToCredit = 0;
                    }

                    disbursementsAndClaimsMaster.IsSettledAmountCredited = false;
                    disbursementsAndClaimsMaster.ApprovalStatusId = (int)EApprovalStatus.Approved;
                    _context.Update(disbursementsAndClaimsMaster);
                    _logger.LogInformation("DisbursementsAndClaimsMaster approve/reject update");


                    try
                    {
                        await _context.DisbursementsAndClaimsMasters.AddAsync(disbursementsAndClaimsMaster);
                        await _context.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "DisbursementsAndClaimsMasters save failed ");
                        return 1;
                    }
                    _logger.LogInformation("DisbursementsAndClaimsMaster table insert complete");
                    _logger.LogInformation("Business Area Request Created successfully");
                    await AtoCashDbContextTransaction.CommitAsync();
                    return 0;
                }
                ///

                try
                {
                    await _context.DisbursementsAndClaimsMasters.AddAsync(disbursementsAndClaimsMaster);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("DisbursementsAndClaimsMaster table insert complete");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message.ToString());
                    return 1;
                }
                await AtoCashDbContextTransaction.CommitAsync();
            }
            _logger.LogInformation("Business Area Request Created successfully");
            return 0;


        }
        #endregion


        //
        private async Task<int> ProjectBasedExpReimRequest(ExpenseReimburseRequestDTO expenseReimburseRequestDto)
        {
            _logger.LogInformation("ProjectBasedExpReimRequest Started");
            //### 1. If Employee Eligible for Cash Claim enter a record and reduce the available amount for next claim
            #region
            using (var AtoCashDbContextTransaction = _context.Database.BeginTransaction())
            {
                int costCenterId = _context.Projects.Find(expenseReimburseRequestDto.ProjectId).CostCenterId;
                int projManagerid = _context.Projects.Find(expenseReimburseRequestDto.ProjectId).ProjectManagerId;
                var approver = _context.Employees.Find(projManagerid);
                int reqEmpid = expenseReimburseRequestDto.EmployeeId;
                bool isSelfApprovedRequest = false;
                double? dblTotalClaimAmount = 0;
                Employee reqEmp = _context.Employees.Find(reqEmpid);

                if (approver != null)
                {
                    _logger.LogInformation("Project Manager defined, no issues");
                }
                else
                {
                    _logger.LogError("Project Manager is not Assigned");
                    return 1;
                }

                ExpenseReimburseRequest expenseReimburseRequest = new();



                _logger.LogInformation("Exp Reimb Table insert started");

                expenseReimburseRequest.ExpenseReportTitle = expenseReimburseRequestDto.ExpenseReportTitle;
                expenseReimburseRequest.BusinessTypeId = null;
                expenseReimburseRequest.BusinessUnitId = null;
                expenseReimburseRequest.EmployeeId = expenseReimburseRequestDto.EmployeeId;
                expenseReimburseRequest.CurrencyTypeId = expenseReimburseRequestDto.CurrencyTypeId;
                expenseReimburseRequest.TotalClaimAmount = dblTotalClaimAmount; //Initially Zero but will be updated after all subclaimes added as per the request
                expenseReimburseRequest.RequestDate = DateTime.UtcNow;
                expenseReimburseRequest.CostCenterId = costCenterId;
                expenseReimburseRequest.ProjectId = expenseReimburseRequestDto.ProjectId;
                expenseReimburseRequest.SubProjectId = expenseReimburseRequestDto.SubProjectId;
                expenseReimburseRequest.WorkTaskId = expenseReimburseRequestDto.WorkTaskId;
                expenseReimburseRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Pending;
                expenseReimburseRequest.ApproverActionDate = null;
                expenseReimburseRequest.Comments = "Expense Reimburse Request in Process!";

                _context.ExpenseReimburseRequests.Add(expenseReimburseRequest); //  <= this generated the Id
                await _context.SaveChangesAsync();

                _logger.LogInformation("Project: Expense Reimburse Table inserted successfully");

                //assign values

                _logger.LogInformation("Sub Claims section started");

                foreach (ExpenseSubClaimDTO expenseSubClaimDto in expenseReimburseRequestDto.ExpenseSubClaims)
                {

                    ExpenseSubClaim expenseSubClaim = new();

                    //get expensereimburserequestId from the saved record and then use here for sub-claims
                    expenseSubClaim.ExpenseReimburseRequestId = expenseReimburseRequest.Id;
                    expenseSubClaim.ExpenseTypeId = expenseSubClaimDto.ExpenseTypeId;
                    expenseSubClaim.EmployeeId = reqEmpid;
                    expenseSubClaim.ExpenseReimbClaimAmount = expenseSubClaimDto.ExpenseReimbClaimAmount;
                    expenseSubClaim.DocumentIDs = expenseSubClaimDto.DocumentIDs;
                    expenseSubClaim.InvoiceNo = expenseSubClaimDto.InvoiceNo;
                    expenseSubClaim.InvoiceDate = expenseSubClaimDto.InvoiceDate;
                    expenseSubClaim.ExpenseCategoryId = expenseSubClaimDto.ExpenseCategoryId;
                    expenseSubClaim.IsVAT = expenseSubClaimDto.IsVAT;
                    expenseSubClaim.TaxNo = expenseSubClaimDto.TaxNo;
                    expenseSubClaim.ExpStrtDate = expenseSubClaimDto.ExpStrtDate;
                    expenseSubClaim.ExpEndDate = expenseSubClaimDto.ExpEndDate;
                    expenseSubClaim.ExpNoOfDays = expenseSubClaimDto.ExpNoOfDays;
                    expenseSubClaim.RequestDate = DateTime.UtcNow;

                    expenseSubClaim.Tax = expenseSubClaimDto.Tax;
                    expenseSubClaim.TaxAmount = expenseSubClaimDto.TaxAmount;
                    expenseSubClaim.VendorId = expenseSubClaimDto.VendorId;
                    expenseSubClaim.AdditionalVendor = expenseSubClaimDto.VendorId == null ? expenseSubClaim.AdditionalVendor : String.Empty;
                    expenseSubClaim.Location = expenseSubClaimDto.Location;

                    expenseSubClaim.BusinessTypeId = null;
                    expenseSubClaim.BusinessUnitId = null;
                    expenseSubClaim.ProjectId = expenseReimburseRequestDto.ProjectId;
                    expenseSubClaim.SubProjectId = expenseReimburseRequestDto.SubProjectId;
                    expenseSubClaim.WorkTaskId = expenseReimburseRequestDto.WorkTaskId;
                    expenseSubClaim.CostCenterId = costCenterId;
                    expenseSubClaim.Description = expenseSubClaimDto.Description;

                    _context.ExpenseSubClaims.Add(expenseSubClaim);

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Sub Claims Table insert failed");
                        return 1;
                    }


                    dblTotalClaimAmount = dblTotalClaimAmount + expenseSubClaimDto.TaxAmount + expenseSubClaimDto.ExpenseReimbClaimAmount;

                }

                ExpenseReimburseRequest exp = _context.ExpenseReimburseRequests.Find(expenseReimburseRequest.Id);

                exp.TotalClaimAmount = dblTotalClaimAmount;
                _context.ExpenseReimburseRequests.Update(exp);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Sub Claims Table records inserted");


                ///////////////////////////// Check if self Approved Request /////////////////////////////
                //if highest approver is requesting Cash Advance request himself
                if (projManagerid == reqEmpid)
                {
                    isSelfApprovedRequest = true;
                }
                //////////////////////////////////////////////////////////////////////////////////////////
                //var test = _context.ApprovalRoleMaps.Include(a => a.ApprovalLevel).ToList().OrderBy(o => o.ApprovalLevel.Level);
                if (isSelfApprovedRequest)
                {
                    _logger.LogInformation("Self Approved Request");
                    ExpenseReimburseStatusTracker expenseReimburseStatusTracker = new()
                    {
                        EmployeeId = reqEmpid,
                        CurrencyTypeId = expenseReimburseRequestDto.CurrencyTypeId,
                        TotalClaimAmount = dblTotalClaimAmount,
                        ExpenseReimburseRequestId = expenseReimburseRequest.Id,
                        BusinessTypeId = null,
                        BusinessUnitId = null,
                        ProjectId = expenseReimburseRequestDto.ProjectId, //Approver Project Id
                        SubProjectId = expenseReimburseRequestDto.SubProjectId,
                        WorkTaskId = expenseReimburseRequestDto.WorkTaskId,
                        JobRoleId = null,
                        ApprovalGroupId = null,
                        ApprovalLevelId = 2,
                        RequestDate = DateTime.UtcNow,
                        ApproverEmpId = reqEmpid,
                        ApproverActionDate = DateTime.UtcNow,
                        ApprovalStatusTypeId = (int)EApprovalStatus.Approved, //1-Pending, 2-Approved, 3-Rejected
                        Comments = "Requestor is highest approver level and is a Approver - Self Approved Request"

                    };
                    _context.ExpenseReimburseStatusTrackers.Add(expenseReimburseStatusTracker);
                    expenseReimburseRequest.ApprovalStatusTypeId = (int)EApprovalStatus.Approved;
                    expenseReimburseRequest.Comments = "Approved";
                    _context.ExpenseReimburseRequests.Update(expenseReimburseRequest);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Self Approved:Expense table Updated with Approved Status");
                }
                else
                {
                    _logger.LogInformation("Not Self approved - Project Manager is Approver");
                    _logger.LogInformation(approver.GetFullName() + " Status Tracker started");


                    ExpenseReimburseStatusTracker expenseReimburseStatusTracker = new()
                    {
                        EmployeeId = reqEmpid,
                        CurrencyTypeId = expenseReimburseRequestDto.CurrencyTypeId,
                        TotalClaimAmount = dblTotalClaimAmount,
                        ExpenseReimburseRequestId = expenseReimburseRequest.Id,
                        BusinessTypeId = null,
                        BusinessUnitId = null,
                        ProjectId = expenseReimburseRequestDto.ProjectId, //Approver Project Id
                        SubProjectId = expenseReimburseRequestDto.SubProjectId,
                        WorkTaskId = expenseReimburseRequestDto.WorkTaskId,
                        JobRoleId = null,
                        ApprovalGroupId = null,
                        ApprovalLevelId = 2,
                        RequestDate = DateTime.UtcNow,
                        ApproverEmpId = reqEmpid,
                        ApproverActionDate = DateTime.UtcNow,
                        ApprovalStatusTypeId = (int)EApprovalStatus.Pending, //1-Pending, 2-Approved, 3-Rejected
                        Comments = "Expense Reimburse is in Process - Pending approval!"

                    };
                    _context.ExpenseReimburseStatusTrackers.Add(expenseReimburseStatusTracker);

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation(approver.GetFullName() + " Status Tracker inserted");

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, " Status Tracker insert failed");
                        return 1;
                    }


                    //##### 5. Send email to the Approver
                    //####################################
                    if (isSelfApprovedRequest)
                    {
                        return 0;
                    }

                    _logger.LogInformation(approver.GetFullName() + "Email Start");

                    string[] paths = { Directory.GetCurrentDirectory(), "ExpApprNotificationEmail.html" };
                    string FilePath = Path.Combine(paths);
                    _logger.LogInformation("Email template path " + FilePath);
                    StreamReader str = new StreamReader(FilePath);
                    string MailText = str.ReadToEnd();
                    str.Close();

                    var approverMailAddress = approver.Email;
                    string subject = expenseReimburseRequest.ExpenseReportTitle + " - #" + expenseReimburseRequest.Id.ToString();
                    Employee emp = _context.Employees.Find(expenseReimburseRequestDto.EmployeeId);

                    var builder = new MimeKit.BodyBuilder();

                    MailText = MailText.Replace("{Requester}", emp.GetFullName());
                    MailText = MailText.Replace("{ApproverName}", approver.GetFullName());
                    MailText = MailText.Replace("{Currency}", _context.CurrencyTypes.Find(emp.CurrencyTypeId).CurrencyCode);
                    MailText = MailText.Replace("{RequestedAmount}", expenseReimburseRequest.TotalClaimAmount.ToString());
                    MailText = MailText.Replace("{RequestNumber}", expenseReimburseRequest.Id.ToString());
                    builder.HtmlBody = MailText;

                    var messagemail = new Message(new string[] { approverMailAddress }, subject, builder.HtmlBody);

                    await _emailSender.SendEmailAsync(messagemail);

                    _logger.LogInformation(approver.GetFullName() + "Email Sent");


                    //repeat for each approver
                }
                #endregion

                //##### 5. Adding a entry in DisbursementsAndClaimsMaster table for records
                #region
                _logger.LogInformation("DisbursementsAndClaimsMaster table Start");
                DisbursementsAndClaimsMaster disbursementsAndClaimsMaster = new();

                disbursementsAndClaimsMaster.EmployeeId = reqEmpid;
                disbursementsAndClaimsMaster.BlendedRequestId = expenseReimburseRequest.Id;
                disbursementsAndClaimsMaster.RequestTypeId = (int)ERequestType.ExpenseReim;
                disbursementsAndClaimsMaster.BusinessTypeId = null;
                disbursementsAndClaimsMaster.BusinessUnitId = null;
                disbursementsAndClaimsMaster.ProjectId = expenseReimburseRequestDto.ProjectId;
                disbursementsAndClaimsMaster.SubProjectId = expenseReimburseRequestDto.SubProjectId;
                disbursementsAndClaimsMaster.WorkTaskId = expenseReimburseRequestDto.WorkTaskId;
                disbursementsAndClaimsMaster.RecordDate = DateTime.UtcNow;
                disbursementsAndClaimsMaster.CurrencyTypeId = expenseReimburseRequestDto.CurrencyTypeId;
                disbursementsAndClaimsMaster.ClaimAmount = dblTotalClaimAmount;
                disbursementsAndClaimsMaster.AmountToCredit = 0; //check this amount NOT zero
                disbursementsAndClaimsMaster.IsSettledAmountCredited = false;
                disbursementsAndClaimsMaster.CostCenterId = costCenterId;
                disbursementsAndClaimsMaster.ApprovalStatusId = isSelfApprovedRequest ? (int)EApprovalStatus.Approved : (int)EApprovalStatus.Pending;

                //save at the end of the code. not here!
                #endregion


                /// #############################
                //   Crediting back to the wallet (for self approvedRequest Only)
                /// #############################
                /// 
                if (isSelfApprovedRequest)
                {
                    double? expenseReimAmt = expenseReimburseRequest.TotalClaimAmount;
                    EmpCurrentCashAdvanceBalance empCashAdvanceBal = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == reqEmpid).FirstOrDefault();
                    double? RoleLimitAmt = empCashAdvanceBal.MaxCashAdvanceLimit;

                    EmpCurrentCashAdvanceBalance empCurrentCashAdvanceBalance = _context.EmpCurrentCashAdvanceBalances.Where(e => e.EmployeeId == reqEmp.Id).FirstOrDefault();
                    double? empCurCashAdvanceBal = empCurrentCashAdvanceBalance.CurBalance;

                    //logic goes here

                    if (expenseReimAmt + empCurCashAdvanceBal >= RoleLimitAmt) // claiming amount is greater than replishable amount
                    {
                        disbursementsAndClaimsMaster.AmountToWallet = RoleLimitAmt - empCurCashAdvanceBal;
                        disbursementsAndClaimsMaster.AmountToCredit = expenseReimAmt - (RoleLimitAmt - empCurCashAdvanceBal);
                    }
                    else
                    {
                        //fully credit to Wallet - Zero amount to bank amount
                        disbursementsAndClaimsMaster.AmountToWallet = expenseReimAmt;
                        disbursementsAndClaimsMaster.AmountToCredit = 0;
                    }


                    disbursementsAndClaimsMaster.ApprovalStatusId = (int)EApprovalStatus.Approved;
                    _context.Update(disbursementsAndClaimsMaster);


                    //////Final Approveer hence update the EmpCurrentCashAdvanceBalance table for the employee to reflect the credit
                    ////empCurrentCashAdvanceBalance.CurBalance = empCurCashAdvanceBal + disbursementsAndClaimsMaster.AmountToWallet ?? 0;
                    ////empCurrentCashAdvanceBalance.UpdatedOn = DateTime.UtcNow;
                    ////_context.EmpCurrentCashAdvanceBalances.Update(empCurrentCashAdvanceBalance);

                    await _context.DisbursementsAndClaimsMasters.AddAsync(disbursementsAndClaimsMaster);

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Project: Self approved Expense request failed");
                        return 1;
                    }

                    _logger.LogInformation("DisbursementsAndClaimsMaster approve/reject updated");
                    return 0;
                }
                ///

                try
                {
                    await _context.DisbursementsAndClaimsMasters.AddAsync(disbursementsAndClaimsMaster);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Project Expense request failed");
                    return 1;
                }

                _logger.LogInformation("DisbursementsAndClaimsMaster approve/reject updated");


                await AtoCashDbContextTransaction.CommitAsync();
            }
            return 0;
        }

    }

}
