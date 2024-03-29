﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoCashAPI.Models
{
    public class CostCenter
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string? CostCenterCode { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? CostCenterDesc{ get; set; }

        [Required]
        [ForeignKey("StatusTypeId")]
        public virtual StatusType? StatusType { get; set; }
        public int StatusTypeId { get; set; }

        public string? GetCostCentre()
        {
            var NameParts = new List<string>();

            NameParts.Add(CostCenterCode ?? "");
            NameParts.Add(CostCenterDesc ?? "");

            //return String.Join(":", FirstName, MiddleName, LastName);

            return String.Join(":", NameParts.Where(s => !String.IsNullOrEmpty(s)));

        }

    }

    public class CostCenterDTO
    {

        public int Id { get; set; }
        public string? CostCenterCode { get; set; }
        public string? CostCenterDesc { get; set; }

        public int StatusTypeId { get; set; }
        public string? StatusType { get; set; }

    }

    public class CostCenterVM
    {
        public int Id { get; set; }
        public string? CostCenterCode { get; set; }


    }
}
