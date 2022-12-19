using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class JobRole
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string? JobRoleCode { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string? JobRoleName { get; set; }

        [Required]
        public Double? MaxPettyCashAllowed { get; set; }


        public string? GetJobRole()
        {
            var NameParts = new List<string>();

            NameParts.Add(JobRoleCode ?? "");
            NameParts.Add(JobRoleName ?? "");


            //return String.Join(" ", FirstName, MiddleName, LastName);

            return String.Join(" ", NameParts.Where(s => !String.IsNullOrEmpty(s)));

        }

    }

    public class JobRoleDTO
    {

        public int Id { get; set; }

        public string? JobRoleCode { get; set; }

        public string? JobRoleName { get; set; }

        public Double? MaxPettyCashAllowed { get; set; }

    }





    public class JobRoleVM
    {

        public int Id { get; set; }
        public string? JobRoleCode { get; set; }

    }


}
