﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{



    public class ApprovalStatusType
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string? Status { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string? StatusDesc { get; set; }


        //    //Pending = 0,
        //    //Approved = 1,
        //    //Rejected = 2

    }
}
