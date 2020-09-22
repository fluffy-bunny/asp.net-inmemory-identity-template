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
        public string IFrameAppId { get; set; }

        public string GuestProxyUrl { get; set; }
        public string GuestUrl { get; set; }
        public string GuestUrl2 { get; private set; }
        public string HostProxyUrl { get; set; }

        public void OnGet(string id)
        {
            var portS = "";
            var port = HttpContext.Request.Host.Port;
            if (port != null)
            {
                portS = $":{port}";
            }

            IFrameAppId = id;
            GuestProxyUrl = $"https://127.0.0.1.xip.io{portS}/GuestProxy/{IFrameAppId}";
            GuestUrl = $"https://127.0.0.1.xip.io{portS}/{IFrameAppId}";
            GuestUrl2 = $"https://a.127.0.0.1.xip.io{portS}/{IFrameAppId}";
            HostProxyUrl = $"https://localhost{portS}/iFrameProxy";

        }
    }
}
