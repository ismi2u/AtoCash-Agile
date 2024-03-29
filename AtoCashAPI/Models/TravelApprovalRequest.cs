﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    [ComplexType]
    public class TravelApprovalRequest
    {


        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int? EmployeeId { get; set; }

        [Required]
        public DateTime? TravelStartDate { get; set; }

        [Required]
        public DateTime? TravelEndDate { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string? TravelPurpose { get; set; }

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

        [Required]
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

    public class TravelApprovalRequestDTO
    {

        public int Id { get; set; }

        public int? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? TravelStartDate { get; set; }
        public DateTime? TravelEndDate { get; set; }
        public string? TravelPurpose { get; set; }
        public DateTime? RequestDate { get; set; }

        public string? BusinessType { get; set; }
        public int? BusinessTypeId { get; set; }

        public string? BusinessUnit { get; set; }
        public string? BusinessUnitCode { get; set; }
        public int? BusinessUnitId { get; set; }

        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int? SubProjectId { get; set; }
        public string? SubProjectName { get; set; }
        public int? WorkTaskId { get; set; }
        public string? WorkTaskName { get; set; }

        public string? Location { get; set; }

        public int? CostCenterId { get; set; }

        public string? CostCenterCode { get; set; }
        public string? CostCenter { get; set; }
        public int? ApprovalStatusTypeId { get; set; }
        public string? ApprovalStatusType { get; set; }
        public DateTime? ApproverActionDate { get; set; }

        public bool ShowEditDelete { get; set; }

        public string? Comments { get; set; }
    }
}
