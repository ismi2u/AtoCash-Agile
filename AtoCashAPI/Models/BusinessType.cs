using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoCashAPI.Models
{
    public class BusinessType
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "varchar(150)")]
        public string? BusinessTypeName { get; set; }
        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? BusinessTypeDesc { get; set; }

        [Required]
        [ForeignKey("StatusTypeId")]
        public virtual StatusType? StatusType { get; set; }
        public int StatusTypeId { get; set; }
    }

    public class BusinessTypeDTO
    {

        public int Id { get; set; }

        public string? BusinessTypeName { get; set; }

        public string? BusinessTypeDesc { get; set; }

        public int StatusTypeId { get; set; }

        public string? StatusType { get; set; }

    }

    public class BusinessTypeVM
    {

        public int Id { get; set; }

        public string? BusinessTypeName { get; set; }


    }
}
