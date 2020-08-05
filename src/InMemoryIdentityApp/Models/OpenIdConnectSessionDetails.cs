﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InMemoryIdentityApp.Models
{
    public class OpenIdConnectSessionDetails
    {
        public string LoginProider { get; set; }
        public Dictionary<string, string> OIDC { get; set; }
    }
}
