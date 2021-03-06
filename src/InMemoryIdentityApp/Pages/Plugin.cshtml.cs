﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InMemoryIdentityApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace InMemoryIdentityApp.Pages
{
    public class PluginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<PluginModel> _logger;

        public string HostPortHole { get; private set; }
        public string Prompt { get; set; }
        public string FrameLoginProxy { get; set; }

      

        public PluginModel(SignInManager<IdentityUser> signInManager, ILogger<PluginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public void OnGet(string hostPortHole )
        {
            HostPortHole = hostPortHole;
            Prompt = "none";
            FrameLoginProxy = $"{Request.Scheme}://{Request.Host}/iFrameLoginProxy";
           
        }
    }
}
