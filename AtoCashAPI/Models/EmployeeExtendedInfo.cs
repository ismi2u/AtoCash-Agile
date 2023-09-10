using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoCashAPI.Models
{
    public class EmployeeExtendedInfo
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int EmployeeId { get; set; }


        [Required]
        [ForeignKey("BusinessTypeId")]
        public virtual BusinessType? BusinessType { get; set; }
        public int? BusinessTypeId { get; set; }

        [Required]
        [ForeignKey("BusinessUnitId")]
        public virtual BusinessUnit? BusinessUnit { get; set; }
        public int? BusinessUnitId { get; set; }

        [Required]
        [ForeignKey("JobRoleId")]
        public virtual JobRole? JobRole { get; set; }
        public int? JobRoleId { get; set; }

        [ForeignKey("ApprovalGroupId")]
        public virtual ApprovalGroup? ApprovalGroup { get; set; }
        public int? ApprovalGroupId { get; set; }

        [Required]
        [ForeignKey("StatusTypeId")]
        public virtual StatusType? StatusType { get; set; }
        public int StatusTypeId { get; set; }
    }

    public class EmployeeExtendedInfoDTO
    {

        public int? Id { get; set; }
        public int EmployeeId { get; set; }

        public string? Employee{ get; set; }
        public int? BusinessTypeId { get; set; }
        public string? BusinessType { get; set; }
        public int? BusinessUnitId { get; set; }
        public string? BusinessUnitCode { get; set; }
        public string? BusinessUnit { get; set; }
        public int? JobRoleId { get; set; }
        public string? JobRoleCode { get; set; }
        public string? JobRole{ get; set; }
        public int? ApprovalGroupId { get; set; }
        public string? ApprovalGroup{ get; set; }
        public int StatusTypeId { get; set; }
        public string? StatusType { get; set; }
    }


    public class EmpExtInfoSearchModel
    {
        public int EmployeeId { get; set; }

        public string? Employee { get; set; }
        public int? BusinessTypeId { get; set; }
        public int? BusinessUnitId { get; set; }
        public int? JobRoleId { get; set; }
        public int? ApprovalGroupId { get; set; }
        public int StatusTypeId { get; set; }
    }

}
