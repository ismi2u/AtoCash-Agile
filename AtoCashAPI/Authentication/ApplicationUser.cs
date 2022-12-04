using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Authentication
{
    public class ApplicationUser: IdentityUser
    {
        public int? EmployeeId { get; set; }
    }
}
