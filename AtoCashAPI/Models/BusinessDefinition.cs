using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoCashAPI.Models
{
    public class BusinessDefinition
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? BusDefinitionName { get; set; }

        [Required]
        [ForeignKey("StatusTypeId")]
        public virtual StatusType? StatusType { get; set; }
        public int? StatusTypeId { get; set; }
    }
}