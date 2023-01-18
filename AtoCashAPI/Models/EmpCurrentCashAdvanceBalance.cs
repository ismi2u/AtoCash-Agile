using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class EmpCurrentCashAdvanceBalance
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public int? EmployeeId { get; set; }

        [Required]
        public Double? MaxCashAdvanceLimit { get; set; }

        public string? AllCashAdvanceLimits { get; set; }


        [Required]
        public Double? CurBalance { get; set; }


        [Required]
        public Double? CashOnHand { get; set; }

        [Required]
        public DateTime? UpdatedOn   { get; set; }

    }


    public class EmpCurrentCashAdvanceBalanceDTO
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public Double? CurBalance { get; set; }

        public DateTime? UpdatedOn { get; set; }

    }



    public class EmpAllCurBalStatusDTO
    {

        public Double? MaxLimit { get; set; }
        public Double? CurBalance { get; set; }
        public Double? CashInHand { get; set; }

        public Double? PendingApprovalCA { get; set; }

        public Double? PendingApprovalER { get; set; }
        public Double? PendingSettlementCA { get; set; }
        public Double? PendingSettlementER { get; set; }
        public DateTime? WalletBalLastUpdated { get; set; }

    }
}
