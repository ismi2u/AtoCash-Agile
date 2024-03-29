﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoCashAPI.Models
{
    public class CashAndClaimRequestSearchModel
    {
        public int? LoggedEmpId { get; set; }
        public int? ReporteeEmpId { get; set; }
        public int? BlendedRequestId { get; set; }
        public int? RequestTypeId { get; set; }
        public int? BusinessTypeId { get; set; }
        public int? BusinessUnitId { get; set; }
        public int? ProjectId{ get; set; }
        public int? SubProjectId { get; set; }
        public int? WorkTaskId { get; set; }
        public DateTime? RecordDateFrom { get; set; }
        public DateTime? RecordDateTo { get; set; }
        public Double? AmountFrom { get; set; }
        public Double? AmountTo { get; set; }
        public int? CostCenterId { get; set; }
        public int? ApprovalStatusId { get; set; }

        public bool IsManager { get; set; }
        public bool? IsAccountSettled { get; set; }


    }



    public class AccountsPayableSearchModel
    {
        public bool? IsAccountSettled { get; set; }
        public DateTime? SettledAccountsFrom { get; set; }
        public DateTime? SettledAccountsTo { get; set; }
        public int? LoggedEmpId { get; set; }
    }






    public class ExpenseSubClaimsSearchModel
    {
        public int? ExpenseTypeId { get; set; }

        public int? LoggedEmpId { get; set; }
        public int? ReporteeEmpId { get; set; }
        public bool IsManager { get; set; }
        public double? ExpenseReimbClaimAmountFrom { get; set; }
        public double? ExpenseReimbClaimAmountTo { get; set; }
        public DateTime? RequestRaisedDateFrom { get; set; }
        public DateTime? RequestRaisedDateTo { get; set; }
        public int? ApprovalStatusTypeId { get; set; }

        public int? CostCenterId { get; set; }

    }

}
