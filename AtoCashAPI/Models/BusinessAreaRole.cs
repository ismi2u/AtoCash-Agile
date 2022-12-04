﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class StoreRole
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


        public string? GetStoreRole()
        {
            var NameParts = new List<string>();

            NameParts.Add(JobRoleCode ?? "");
            NameParts.Add(JobRoleName ?? "");
            
            return String.Join(":", NameParts.Where(s => !String.IsNullOrEmpty(s)));

        }

    }

    public class StoreRoleDTO
    {

        public int? Id { get; set; }

        public string? BARoleCode { get; set; }

        public string? BARoleName { get; set; }

        public Double? MaxPettyCashAllowed { get; set; }

    }



    public class StoreRoleVM
    {

        public int? Id { get; set; }
        public string? JobRoleCode { get; set; }

    }


}
