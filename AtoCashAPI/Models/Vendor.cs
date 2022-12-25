using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AtoCashAPI.Models
{
    public class Vendor
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? VendorName { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? City { get; set; }


        [Required]
        [Column(TypeName = "varchar(400)")]
        public string? Description{ get; set; }


        [Required]
        [ForeignKey("StatusTypeId")]
        public virtual StatusType? StatusType { get; set; }
        public int StatusTypeId { get; set; }
    }



    public class VendorVM
    {
        public int Id { get; set; }

        public string? VendorName { get; set; }
    }


    public class VendorDTO
    {
        public int Id { get; set; }
        public string? VendorName { get; set; }
        public string? City { get; set; }
        public string? Description { get; set; }
        public int StatusTypeId { get; set; }

        public string? StatusType { get; set; }
    }
}