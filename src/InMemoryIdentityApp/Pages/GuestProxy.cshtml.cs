using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace InMemoryIdentityApp.Pages
{
    public class GuestProxyModel : PageModel
    {
        private ILogger<GuestProxyModel> _logger;

        public GuestProxyModel(ILogger<GuestProxyModel> logger)
        {
            _logger = logger;
        }
        public string GuestId { get; private set; }

        public void OnGet(string id)
        {
            GuestId = id;
            _logger.LogInformation($"GuestProxy: id={GuestId}");
        }
    }
}
