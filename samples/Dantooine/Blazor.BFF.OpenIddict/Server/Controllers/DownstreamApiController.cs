using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blazor.BFF.OpenIddict.Server.Controllers
{
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class DownstreamApiController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new List<string> { "some data", "more data", "loads of data" };
        }
    }
}
