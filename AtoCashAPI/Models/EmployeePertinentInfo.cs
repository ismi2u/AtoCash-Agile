using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoCashAPI.Models
{
    public class EmployeePertinentInfo
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int? EmployeeId { get; set; }

        [Required]
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
        public int? DepartmentId { get; set; }


        [ForeignKey("DepartmentRoleId")]
        public virtual DepartmentRole? DepartmentRole { get; set; }
        public int? DepartmentRoleId { get; set; }




        [ForeignKey("ApprovalGroupId")]
        public virtual ApprovalGroup? ApprovalGroup { get; set; }
        public int? ApprovalGroupId { get; set; }

        [Required]
        [ForeignKey("BusinessAreaId")]
        public virtual BusinessArea? BusinessArea { get; set; }
        public int? BusinessAreaId { get; set; }


        [ForeignKey("BusinessAreaRoleId")]
        public virtual BusinessAreaRole? BusinessAreaRole { get; set; }
        public int? BusinessAreaRoleId { get; set; }



        [Required]
        [ForeignKey("CurrencyTypeId")]
        public virtual CurrencyType? CurrencyType { get; set; }
        public int? CurrencyTypeId { get; set; }
    }
}
