﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AtoCashAPI.Models
{
    public class WorkTask
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("SubProjectId")]
        public virtual SubProject? SubProject { get; set; }
        public int? SubProjectId { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string? TaskName { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string? TaskDesc { get; set; }

    }

    public class WorkTaskDTO
    {
        public int Id { get; set; }
        public int? SubProjectId { get; set; }
        public string? SubProject { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDesc { get; set; }

    }

    public class WorkTaskVM
    {
        public int Id { get; set; }
        public string? TaskName { get; set; }
    }
}
