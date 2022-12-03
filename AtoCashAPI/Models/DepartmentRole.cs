using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class DepartmentRole
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string? JobRoleCode { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string? JobRoleName { get; set; }

        [Required]
        public Double? MaxPettyCashAllowed { get; set; }


        public string? GetDepartmentRole()
        {
            var NameParts = new List<string>();

            NameParts.Add(JobRoleCode ?? "");
            NameParts.Add(JobRoleName ?? "");

            return String.Join(":", NameParts.Where(s => !String.IsNullOrEmpty(s)));

        }

    }

    public class DeptartmentRoleDTO
    {
        public int? Id { get; set; }
        public string? JobRoleCode { get; set; }
        public string? JobRoleName{ get; set; }
        public Double? MaxPettyCashAllowed { get; set; }

    }





    public class DeptartmentRoleVM
    {

        public int? Id { get; set; }
        public string? JobRoleCode { get; set; }

    }


}
