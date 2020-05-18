using System.Security.Claims;
using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Zirku.Api2.Controllers
{
    public class ResourceController : Controller
    {
        [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
        [HttpGet]
        public IActionResult Private()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return BadRequest();
            }

            return Content($"ResourceServer2 says that you have authorized access to resources belonging to {identity.Name}.");
        }

        [HttpGet]
        public IActionResult Public()
        {
            return Content("This is a public endpoint at Zirku.Api2 that does not require authorization.");
        }
    }
}
