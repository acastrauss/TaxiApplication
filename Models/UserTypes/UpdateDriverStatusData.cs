﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.UserTypes
{
    public class UpdateDriverStatusData
    {
        public string Email { get; set; }
        public DriverStatus Status { get; set; }
    }
}
