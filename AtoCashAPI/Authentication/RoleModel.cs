﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtoCashAPI.Authentication
{
    public class RoleModel
    {
        public string? RoleName { get; set; }
    }

    public class RoleToUserSearch
    {
        public string? RoleId { get; set; }
    }

}
