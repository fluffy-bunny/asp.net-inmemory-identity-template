 
using InMemoryIdentityApp.Constants;
using InMemoryIdentityApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InMemoryIdentityApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PingController : ControllerBase
    {

        private readonly ILogger<PingController> _logger;

        public PingController(ILogger<PingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task OnGetAsync()
        {
        }
    }
}
