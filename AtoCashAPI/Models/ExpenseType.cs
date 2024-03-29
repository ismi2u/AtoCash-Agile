﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoCashAPI.Models
{
    public class ExpenseType
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string? ExpenseTypeName { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? ExpenseTypeDesc { get; set; }

        [Required]
        [ForeignKey("ExpenseCategoryId")]
        public virtual ExpenseCategory? ExpenseCategory { get; set; }
        public int? ExpenseCategoryId { get; set; }

        [Required]
        [ForeignKey("GeneralLedgerId")]
        public virtual GeneralLedger? GeneralLedger { get; set; }
        public int? GeneralLedgerId { get; set; }


        [Required]
        [ForeignKey("StatusTypeId")]
        public virtual StatusType? StatusType { get; set; }
        public int StatusTypeId { get; set; }

    }


    public class ExpenseTypeDTO
    {

        public int Id { get; set; }
        public string? ExpenseTypeName { get; set; }
        public string? ExpenseTypeDesc { get; set; }

        public int? ExpenseCategoryId { get; set; }
        public string? ExpenseCategoryName { get; set; }
        public string? ExpenseCategoryDesc { get; set; }

        public int? GeneralLedgerId { get; set; }
        public string? GeneralLedgerAccountNo { get; set; }
        public string? GeneralLedgerAccountName { get; set; }


        public string? StatusType { get; set; }
        public int StatusTypeId { get; set; }

    }


    public class ExpenseTypeVM
    {
        public int Id { get; set; }
        public string? ExpenseTypeName { get; set; }

    }
}
