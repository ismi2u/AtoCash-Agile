using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class CashAdvanceStatusTracker
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int? EmployeeId { get; set; }


        [ForeignKey("BusinessTypeId")]
        public virtual BusinessType? BusinessType { get; set; }
        public int? BusinessTypeId { get; set; }


        [ForeignKey("BusinessUnitId")]
        public virtual BusinessUnit? BusinessUnit { get; set; }
        public int? BusinessUnitId { get; set; }

        [Required]
        [ForeignKey("CashAdvanceRequestId")]
        public virtual CashAdvanceRequest? CashAdvanceRequest { get; set; }
        public int? CashAdvanceRequestId { get; set; }


        [ForeignKey("ProjManagerId")]
        public virtual Employee? ProjectManager { get; set; }
        public int? ProjManagerId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }
        public int? ProjectId { get; set; }


        [ForeignKey("SubProjectId")]
        public virtual SubProject? SubProject { get; set; }
        public int? SubProjectId { get; set; }


        [ForeignKey("WorkTaskId")]
        public virtual WorkTask? WorkTask { get; set; }
        public int? WorkTaskId { get; set; }

        public int? ApprovalGroupId { get; set; }

        //Approver Role
        [ForeignKey("JobRoleId")]
        public virtual JobRole? JobRole { get; set; }
        public int? JobRoleId { get; set; }

        //Approver ApprovalLevel

        [ForeignKey("ApprovalLevelId")]
        public virtual ApprovalLevel? ApprovalLevel { get; set; }
        public int? ApprovalLevelId { get; set; }


   

        [Required]
        public DateTime? RequestedDate { get; set; }


        [ForeignKey("ApproverEmpId")]
        public virtual Employee? Approver { get; set; }
        public int? ApproverEmpId { get; set; }

        public DateTime? ApproverActionDate { get; set; }

        [Required]
        [ForeignKey("ApprovalStatusTypeId")]
        public virtual ApprovalStatusType? ApprovalStatusType { get; set; }
        public int? ApprovalStatusTypeId { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? Comments { get; set; }
    }


    public class CashAdvanceStatusTrackerDTO
    {
        public int Id { get; set; }

        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        public int? CashAdvanceRequestId { get; set; }

        public string? BusinessType{ get; set; }
        public int? BusinessTypeId { get; set; }

        public string? BusinessUnit { get; set; }
        public int? BusinessUnitId { get; set; }

        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }

        public int? SubProjectId { get; set; }
        public string? SubProjectName { get; set; }

        public int? WorkTaskId { get; set; }
        public string? WorkTask { get; set; }


        public int? ApprovalGroupId { get; set; }

        public int? JobRoleId { get; set; }
        public string? JobRole { get; set; }

        public int? ApprovalLevelId { get; set; }

        public DateTime? RequestedDate { get; set; }

        public DateTime? ApproverActionDate { get; set; }

        public int? ApprovalStatusTypeId { get; set; }
        public string? ApprovalStatusType { get; set; }

        public Double? ClaimAmount { get; set; }
        public string? Comments { get; set; }

    }
}
