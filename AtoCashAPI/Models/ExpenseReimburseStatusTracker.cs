using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class ExpenseReimburseStatusTracker
    { 

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int? EmployeeId { get; set; }

        [Required]
        [ForeignKey("ExpenseReimburseRequestId")]
        public virtual ExpenseReimburseRequest? ExpenseReimburseRequest { get; set; }
        public int? ExpenseReimburseRequestId { get; set; }


        [Required]
        [ForeignKey("CurrencyTypeId")]
        public virtual CurrencyType? CurrencyType { get; set; }
        public int? CurrencyTypeId { get; set; }

        [Required]
        public Double? TotalClaimAmount { get; set; }

        [Required]
        public DateTime? ExpReimReqDate { get; set; }
        //[Required]
        //public bool IsStoreReq { get; set; }

        ///// 

        //[ForeignKey("StoreId")]
        //public virtual Store? Store { get; set; }
        //public int? StoreId { get; set; }

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

        [ForeignKey("ApprovalGroupId")]
        public virtual ApprovalGroup? ApprovalGroup { get; set; }
        public int? ApprovalGroupId { get; set; }


        //Approver Role
        [Required]
        [ForeignKey("JobRoleId")]
        public virtual JobRole? JobRole { get; set; }
        public int? JobRoleId { get; set; }


        [Required]
        [ForeignKey("ApprovalLevelId")]
        public virtual ApprovalLevel? ApprovalLevel { get; set; }
        public int? ApprovalLevelId { get; set; }


        [Required]
        [ForeignKey("ApprovalStatusTypeId")]
        public virtual ApprovalStatusType? ApprovalStatusType { get; set; }
        public int? ApprovalStatusTypeId { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? Comments { get; set; }
    }




    public class ExpenseReimburseStatusTrackerDTO
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public int? ExpenseReimburseRequestId { get; set; }
        public int? CurrencyTypeId { get; set; }
        public string? CurrencyType { get; set; }
        public Double? TotalClaimAmount { get; set; }
        public DateTime? ExpReimReqDate { get; set; }

        public int? StoreId { get; set; }
        public string? Store{ get; set; }

        public bool IsStoreReq { get; set; }
        public int? DepartmentId { get; set; }
        public string? Department { get; set; }
        public int? ProjectId { get; set; }
        public string? Project { get; set; }

        public int? WorkTaskId { get; set; }
        public string? WorkTask { get; set; }

        public int? SubProjectId { get; set; }
        public string? SubProject { get; set; }


        public int? ApprovalGroupId { get; set; }
        public int? ApprovalLevelId { get; set; }

        public int? JobRoleId { get; set; }
        public string? JobRole { get; set; }

        public int? BARoleId { get; set; }
        public string? BAJobRole { get; set; }

        public int? BAApprovalGroupId { get; set; }
        public string? BAApprovalGroup { get; set; }

        public string? ApprovalStatusType { get; set; }
        public int? ApprovalStatusTypeId { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public string? Comments { get; set; }




    }
}
