﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class TravelRequestSearchModel
    {

        public int Id { get; set; }
        public int? LoggedEmpId { get; set; }
        public int? ReporteeEmpId { get; set; }
        public string? EmployeeName { get; set; }
        public int? TravelApprovalRequestId { get; set; }

        public DateTime? TravelStartDate { get; set; }
        public DateTime? TravelEndDate { get; set; }

        public string? TravelPurpose { get; set; }

        public int? BusinessUnitId { get; set; }
        public string? BusinessUnit { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }

        public string? SubProjectName { get; set; }
        public int? SubProjectId { get; set; }

        public string? WorkTaskName { get; set; }
        public int? WorkTaskId { get; set; }
        public DateTime? ReqRaisedDate { get; set; }
  
        public int? CostCenterId { get; set; }
        public string? CostCenter { get; set; }
        public int? ApprovalStatusTypeId { get; set; }

        public string? ApprovalStatusType { get; set; }

        public bool IsManager { get; set; }



    }
}
