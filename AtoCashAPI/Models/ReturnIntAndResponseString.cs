using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoCashAPI.Models
{
    public class ReturnIntAndResponseString
    {
        public int IntReturn{ get; set; }
        public string? StrResponse { get; set; }
    }


}
