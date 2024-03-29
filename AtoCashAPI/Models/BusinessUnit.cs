﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AtoCashAPI.Models
{
    public class BusinessUnit
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [Required]
        [ForeignKey("BusinessTypeId")]
        public virtual BusinessType? BusinessType { get; set; }
        public int? BusinessTypeId { get; set; }


        [Column(TypeName = "varchar(250)")]
        public string? BusinessUnitCode { get; set; }


        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? BusinessUnitName{ get; set; }

        [Required]
        [ForeignKey("CostCenterId")]
        public virtual CostCenter? CostCenter { get; set; }
        public int? CostCenterId { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? BusinessDesc { get; set; }


        [Required]
        [ForeignKey("LocationId")]
        public virtual Location? Location { get; set; }
        public int? LocationId { get; set; }


        [Required]
        [ForeignKey("StatusTypeId")]
        public virtual StatusType? StatusType { get; set; }
        public int StatusTypeId { get; set; }

        public string? GetBusinessUnitName()
        {
            var NameParts = new List<string>();

            NameParts.Add(BusinessUnitName ?? "");
            NameParts.Add(BusinessUnitCode ?? "");


            //return String.Join(" ", FirstName, MiddleName, LastName);

            return String.Join(":", NameParts.Where(s => !String.IsNullOrEmpty(s)));

        }

    }


    public class BusinessUnitDTO
    {
        public int? Id { get; set; }
        public int? BusinessTypeId { get; set; }
        public string? BusinessType { get; set; }
        public string? BusinessUnitCode{ get; set; }

        public string? BusinessUnitName { get; set; }
        public int? CostCenterId { get; set; }
        public string? CostCenter { get; set; }
        public string? BusinessDesc { get; set; }
        public int? LocationId { get; set; }
        public string? Location { get; set; }
        public int StatusTypeId { get; set; }

        public string? StatusType { get; set; }
    }


    public class BusinessUnitVM
    {
        public int? Id { get; set; }
        public string? BusinessUnitName { get; set; }
     
    }


   
}