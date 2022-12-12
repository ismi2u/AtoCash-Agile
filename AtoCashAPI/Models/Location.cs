using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AtoCashAPI.Models
{
    public class Location
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? LocationName { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? City { get; set; }


        [Precision(18, 6)]
        public Decimal? Lattitude { get; set; }


        [Precision(18, 6)]
        public Decimal? Longitude { get; set; }

        [Required]
        [Column(TypeName = "varchar(400)")]
        public string? LocationDesc { get; set; }


        [Required]
        [ForeignKey("StatusTypeId")]
        public virtual StatusType? StatusType { get; set; }
        public int? StatusTypeId { get; set; }
    }
}