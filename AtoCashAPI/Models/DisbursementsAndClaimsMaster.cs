﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class DisbursementsAndClaimsMaster
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int? EmployeeId { get; set; }


        [ForeignKey("PettyCashRequestId")]
        public virtual PettyCashRequest? PettyCashRequest { get; set; }

        public int? PettyCashRequestId { get; set; }


        [ForeignKey("ExpenseReimburseReqId")]
        public virtual ExpenseReimburseRequest? ExpenseReimburseRequest { get; set; }
        public int? ExpenseReimburseReqId { get; set; }

        [Required]
        [ForeignKey("RequestTypeId")]
        public virtual RequestType? RequestType { get; set; }
        public int? RequestTypeId { get; set; }


         [ForeignKey("BusinessUnitId")]
        public virtual BusinessUnit? BusinessUnit { get; set; }
        public int? BusinessUnitId { get; set; }


        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }
        public int? ProjectId { get; set; }

        [ForeignKey("SubProjectId")]
        public virtual SubProject? SubProject { get; set; }
        public int? SubProjectId { get; set; }

        [ForeignKey("WorkTaskId")]
        public virtual WorkTask? WorkTask { get; set; }
        public int? WorkTaskId { get; set; }

        [Required]
        public DateTime? RecordDate { get; set; }


        [Required]
        [ForeignKey("CurrencyTypeId")]
        public virtual CurrencyType? CurrencyType { get; set; }
        public int? CurrencyTypeId { get; set; }



        [Required]
        public Double? ClaimAmount { get; set; }


        public Double? AmountToWallet { get; set; }


        public Double? AmountToCredit { get; set; }

        public bool? IsSettledAmountCredited { get; set; }
        public DateTime? SettledDate { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string? SettlementComment { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string? SettlementAccount { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string? SettlementBankCard { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string? AdditionalData { get; set; }


        [Required]
        [ForeignKey("CostCenterId")]
        public virtual CostCenter? CostCenter { get; set; }
        public int? CostCenterId { get; set; }

        [Required]
        [ForeignKey("ApprovalStatusId")]
        public ApprovalStatusType? ApprovalStatusType { get; set; }
        public int? ApprovalStatusId { get; set; }



    }


    public class PostSAPAPIData
    {
        public int? ClaimId { get; set; }

        public string? EmployeeName { get; set; }
        public string? EmployeeCode{ get; set; }
        public string? BusinessUnit { get; set; }
        public string? Project { get; set; }
        public DateTime? RequestDate { get; set; }
        public Double? ClaimAmount { get; set; }
        public double? AmountToWallet { get; set; }
        public double? AmountToBank { get; set; }
        public string? Status { get; set; }
        public  bool IsCashAdvanceRequest { get; set; }
        public List<PostSubClaimItems>? SubClaimItems { get; set; }
       
    }

    public class PostSubClaimItems
    {
        public int? SubClaimId { get; set; }
        public string? CostCentre { get; set; }
        public string? GeneralLedger { get; set; }
        public Double? SubClaimAmount { get; set; }
        public string? Vendor { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? ExpenseType { get; set; }
       
    }

    public class ResponseSAPApiData
    {
        public string? AdditionalData { get; set; }
        public string? SettlementComment { get; set; }
        public bool isSettled { get; set; }

    }


    public class DisbursementsAndClaimsMasterDTO
    {

        public int? Id { get; set; }
        public int? EmployeeId { get; set; }

        public string? EmployeeName { get; set; }
        public int? PettyCashRequestId { get; set; }
        public int? ExpenseReimburseReqId { get; set; }

        public int? RequestTypeId { get; set; }
        public string? RequestType { get; set; }

        public int? BusinessUnitId { get; set; }
        public string? BusinessUnitName { get; set; }

 
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }

        public string? SubProjectName { get; set; }
        public int? SubProjectId { get; set; }

        public string? WorkTaskName { get; set; }
        public int? WorkTaskId { get; set; }
        public string? RecordDate { get; set; }

        public int? CurrencyTypeId { get; set; }

        public string? CurrencyType { get; set; }
        public Double? ClaimAmount { get; set; }
        public Double? AmountToWallet { get; set; }
        public Double? AmountToCredit { get; set; }



        public bool IsSettledAmountCredited { get; set; }
        public string? SettledDate { get; set; }
        public string? SettlementComment { get; set; }

        public string? SettlementAccount { get; set; }

        public string? SettlementBankCard { get; set; }

        public string? AdditionalData { get; set; }


        public int? CostCenterId { get; set; }
        public string? CostCenter { get; set; }
        public int? ApprovalStatusId { get; set; }

        public string? ApprovalStatusType { get; set; }

        public string? RequestTitleDescription { get; set; }

    }
}