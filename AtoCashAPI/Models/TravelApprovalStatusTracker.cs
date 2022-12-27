using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class TravelApprovalStatusTracker
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int? EmployeeId { get; set; }


        [ForeignKey("TravelApprovalRequestId")]
        public virtual TravelApprovalRequest TravelApprovalRequest { get; set; }

        public int? TravelApprovalRequestId { get; set; }


        public DateTime? TravelStartDate { get; set; }

        public DateTime? TravelEndDate { get; set; }


        [ForeignKey("BusinessTypeId")]
        public virtual BusinessType? BusinessType { get; set; }
        public int? BusinessTypeId { get; set; }



        [ForeignKey("BusinessUnitId")]
        public virtual BusinessUnit? BusinessUnit { get; set; }
        public int? BusinessUnitId { get; set; }


        [ForeignKey("ProjManagerId")]
        public virtual Employee? ProjManager { get; set; }
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


        //Approver ApprovalLevel
         public int? ApprovalGroupId { get; set; }


        //Approver Role => Multiple Approvers, hence their roles
 
        [ForeignKey("JobRoleId")]
        public virtual JobRole? JobRole { get; set; }
        public int? JobRoleId { get; set; }

        //Approver ApprovalLevel
        [Required]
        [ForeignKey("ApprovalLevelId")]
        public virtual ApprovalLevel ApprovalLevel { get; set; }
        public int? ApprovalLevelId { get; set; }

        [Required]
        public DateTime? RequestDate { get; set; }

        [ForeignKey("ApproverEmpId")]
        public virtual Employee? Approver { get; set; }
        public int? ApproverEmpId { get; set; }


        public DateTime? ApproverActionDate { get; set; }

        [Required]
        [ForeignKey("ApprovalStatusTypeId")]
        public virtual ApprovalStatusType? ApprovalStatusType { get; set; }
        public int? ApprovalStatusTypeId { get; set; }


        [Column(TypeName = "varchar(250)")]
        public string? Comments { get; set; }

    }

    public class TravelApprovalStatusTrackerDTO
    {

        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public int? TravelApprovalRequestId { get; set; }


        public DateTime? TravelStartDate { get; set; }

        public DateTime? TravelEndDate { get; set; }
        public string? BusinessType { get; set; }
        public int? BusinessTypeId { get; set; }

        public string? BusinessUnit { get; set; }
        public int? BusinessUnitId { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }

        public int? ApprovalGroupId { get; set; }
        public int? JobRoleId { get; set; }
        public string? JobRole { get; set; }
        public int? ApprovalLevelId { get; set; }
        public DateTime? RequestDate { get; set; }

        public int? ApproverEmpId { get; set; }
        public DateTime? ApproverActionDate { get; set; }
        public int? ApprovalStatusTypeId { get; set; }

        public string? ApprovalStatusType { get; set; }

        public string? Comments { get; set; }

    }
}
