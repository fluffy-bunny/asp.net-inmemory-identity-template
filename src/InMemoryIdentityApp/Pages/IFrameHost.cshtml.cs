using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace InMemoryIdentityApp.Pages
{
    public class IFrameHostModel : PageModel
    {
        private readonly ILogger<IFrameHostModel> _logger;

        public IFrameHostModel(ILogger<IFrameHostModel> logger)
        {
            _logger = logger;
        }
        public string IFrameAppId;

        public void OnGet(string id)
        {
            IFrameAppId = id;
        }
    }
}
