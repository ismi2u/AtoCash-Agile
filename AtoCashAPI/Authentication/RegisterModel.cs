﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Authentication
{
    public class RegisterModel
    {
        public int? EmployeeId { get; set; }
        public  string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

    }
}
