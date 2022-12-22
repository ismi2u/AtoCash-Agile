using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoCashAPI.Models
{
    public class ApprovalRoleMap
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("ApprovalGroupId")]
        public virtual ApprovalGroup? ApprovalGroup { get; set; }
        public int? ApprovalGroupId { get; set; }

        [Required]
        [ForeignKey("JobRoleId")]
        public virtual JobRole? JobRole { get; set; }
        public int? JobRoleId { get; set; }

        [Required]
        [ForeignKey("ApprovalLevelId")]
        public virtual ApprovalLevel? ApprovalLevel { get; set; }
        public int? ApprovalLevelId { get; set; }
    }

    public class ApprovalRoleMapDTO
    {
        public int? Id { get; set; }

        public string? ApprovalGroup { get; set; }
        public int? ApprovalGroupId { get; set; }
        public string? JobRole { get; set; }
        public int? JobRoleId { get; set; }

        public int? ApprovalLevel { get; set; }
        public int? ApprovalLevelId { get; set; }

        public string? EmployeeName { get; set; }

        public int? CountOfEmployees { get; set; }
        public string? AllAssignedEmployees { get; set; }

    }


}
