using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Models
{
    public class ResetPasswordVM
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? email { get; set; }

        [Required]
        public string? Password { get; set; }

        public string? Token { get; set; }
    }
}
