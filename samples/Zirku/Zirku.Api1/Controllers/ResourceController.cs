using System.Security.Claims;
using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Zirku.Api1.Controllers
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

            return Content($"You have authorized access to resources belonging to {identity.Name} on Zirku.Api01.");
        }

        [HttpGet]
        public IActionResult Public()
        {
            return Content("This is a public endpoint that is at Zirku.Api01; it does not require authorization.");
        }
    }
}
