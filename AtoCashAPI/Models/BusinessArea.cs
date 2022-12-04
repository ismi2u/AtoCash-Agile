using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoCashAPI.Models
{
    public class Store
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        [Required]
        [Column(TypeName = "varchar(150)")]
        public string? StoreCode { get; set; }
        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? StoreName { get; set; }

   
        [Required]
        [ForeignKey("CostCenterId")]
        public virtual CostCenter? CostCenter { get; set; }
        public int? CostCenterId { get; set; }


        [Required]
        [ForeignKey("StatusTypeId")]
        public virtual StatusType? StatusType { get; set; }
        public int? StatusTypeId { get; set; }


        public string? GetStore()
        {
            var NameParts = new List<string>();

            NameParts.Add(StoreCode ?? "");
            NameParts.Add(StoreName ?? "");

            return String.Join(":", NameParts.Where(s => !String.IsNullOrEmpty(s)));

        }
    }

    public class StoreDTO
    {

        public int? Id { get; set; }

        public string? StoreCode { get; set; }

        public string? StoreName { get; set; }


        public int? CostCenterId { get; set; }

        public string? CostCenter { get; set; }

        public string? StatusType { get; set; }

        public int? StatusTypeId { get; set; }

    }

    public class StoreVM
    {

        public int? Id { get; set; }

        public string? StoreCode { get; set; }
        public string? StoreName { get; set; }

    }
}
