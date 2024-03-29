﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class ExpenseSubClaim
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [Required]
        [ForeignKey("ExpenseReimburseRequestId")]
        public virtual ExpenseReimburseRequest? ExpenseReimburseRequest { get; set; }
        public int? ExpenseReimburseRequestId { get; set; }

        [Required]
        [ForeignKey("ExpenseCategoryId")]
        public virtual ExpenseCategory? ExpenseCategory { get; set; }
        public int? ExpenseCategoryId { get; set; }


        [Required]
        [ForeignKey("ExpenseTypeId")]
        public virtual ExpenseType? ExpenseType { get; set; }
        public int? ExpenseTypeId { get; set; }


        public DateTime? ExpStrtDate { get; set; }
 
        public DateTime? ExpEndDate { get; set; }

        public int? ExpNoOfDays { get; set; }

        public string? TaxNo { get; set; }


        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int? EmployeeId { get; set; }

        [Required]
        public DateTime? RequestDate { get; set; }

        [Required]
        public Double? ExpenseReimbClaimAmount { get; set; }

        public string? DocumentIDs { get; set; }


        [Required]
        [Column(TypeName = "varchar(100)")]
        public string? InvoiceNo { get; set; }


        [Required]

        public bool IsVAT { get; set; }


        [Required]

        public float Tax { get; set; }

        [Required]
        public Double? TaxAmount { get; set; }

        [Required]
        public DateTime? InvoiceDate { get; set; }  
        

        [ForeignKey("VendorId")]
        public virtual Vendor? Vendor { get; set; }
        public int? VendorId { get; set; }

        public string? AdditionalVendor { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? Location { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? Description { get; set; }

        [ForeignKey("BusinessTypeId")]
        public virtual BusinessType? BusinessType { get; set; }
        public int? BusinessTypeId { get; set; }

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
        [ForeignKey("CostCenterId")]
        public virtual CostCenter? CostCenter { get; set; }
        public int? CostCenterId { get; set; }

    }

    public class ExpenseSubClaimDTO
    {

        public int Id { get; set; }

        public string? EmployeeName { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }

        public int? ExpenseReimburseRequestId { get; set; }
        public int? ExpenseCategoryId { get; set; }

        public string? ExpenseCategory { get; set; }

        public DateTime? ExpStrtDate { get; set; }
        public DateTime? ExpEndDate { get; set; }
        public int? ExpNoOfDays { get; set; }

        public bool IsVAT { get; set; }
        public string? TaxNo { get; set; }
        public Double? ExpenseReimbClaimAmount { get; set; }

        public string? DocumentIDs { get; set; }

        public DateTime? RequestDate { get; set; }

        public string? InvoiceNo { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public float Tax { get; set; }

        public Double? TaxAmount { get; set; }

        public int? VendorId { get; set; }

        public string? Vendor { get; set; }

        public string? AdditionalVendor { get; set; }

        public string? Location { get; set; }

        public string? Description { get; set; }

        public bool IsStoreReq { get; set; }
        //Foreign Key Relationsions

        public int? CurrencyTypeId { get; set; }
        public string? CurrencyType { get; set; }
        public int? ExpenseTypeId { get; set; }
        public string? ExpenseType { get; set; }

        public int? GeneralLedgerId { get; set; }
        public string? GeneralLedger { get; set; }
        public string? GeneralLedgerAccountNo { get; set; }

        public int? BusinessTypeId { get; set; }
        public string? BusinessType { get; set; }
        public int? BusinessUnitId { get; set; }
        public string? BusinessUnitCode { get; set; }
        public string? BusinessUnit { get; set; }


        public string? ProjectName { get; set; }
        public int? ProjectId { get; set; }

        public string? SubProjectName  { get; set; }
        public int? SubProjectId { get; set; }

        public string? WorkTaskName  { get; set; }
        public int? WorkTaskId { get; set; }

        public int? CostCenterId { get; set; }
        public string? CostCenter { get; set; }
        public string? CostCenterCode { get; set; }
        public string? ApprovalStatusType { get; set; }
        public int? ApprovalStatusTypeId { get; set; }


        public DateTime? ApproverActionDate { get; set; }

        public string? PostingDate { get; set; }
        public string? ApproverId { get; set; }

    }
}
