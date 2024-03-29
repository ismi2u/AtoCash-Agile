﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class ExpenseReimburseRequest
    { 

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


   

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? ExpenseReportTitle { get; set; }

      
        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int EmployeeId { get; set; }

        [Required]
        [ForeignKey("CurrencyTypeId")]
        public virtual CurrencyType? CurrencyType { get; set; }
        public int? CurrencyTypeId { get; set; }

        [Required]
        public Double? TotalClaimAmount { get; set; }

        [Required]
        public DateTime? RequestDate { get; set; }


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

        [ForeignKey("CostCenterId")]
        public virtual CostCenter? CostCenter { get; set; }
        public int? CostCenterId { get; set; }

        [Required]
        [ForeignKey("ApprovalStatusTypeId")]
        public virtual ApprovalStatusType? ApprovalStatusType { get; set; }
        public int? ApprovalStatusTypeId { get; set; }

        public DateTime? ApproverActionDate { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? Comments { get; set; }
    }




    public class ExpenseReimburseRequestDTO
    {
        public int Id { get; set; }

        public string? ExpenseReportTitle { get; set; }
        public int EmployeeId { get; set; }

       
        public string? EmployeeName { get; set; }
        public int? CurrencyTypeId { get; set; }
        public Double? TotalClaimAmount { get; set; }

        public int? BusinessTypeId { get; set; }
        public string? BusinessType { get; set; }

        public int? BusinessUnitId { get; set; }
        public string? BusinessUnit { get; set; }
        public string? ProjectName { get; set; }
        public int? ProjectId { get; set; }

        public string? SubProjectName { get; set; }
        public int? SubProjectId { get; set; }

        public string? WorkTaskName { get; set; }
        public int? WorkTaskId { get; set; }

        public int? CostCenterId { get; set; }
        public string? CostCentre { get; set; }
        public int? ApprovalStatusTypeId { get; set; }
        public string? ApprovalStatusType { get; set; }

        public DateTime? RequestDate { get; set; }
        public DateTime? ApproverActionDate { get; set; }

        public string? Location { get; set; }

        public bool ShowEditDelete { get; set; }
        public List<ExpenseSubClaimDTO> ExpenseSubClaims { get; set; }

        public double? CreditToWallet { get; set; }

        public double? CreditToBank { get; set; }

        public bool IsSettled { get; set; }

        public string? Comments { get; set; }


    }
}
