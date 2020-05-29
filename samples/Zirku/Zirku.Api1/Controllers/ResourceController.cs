using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace Zirku.Api1.Controllers
{
    public class ResourceController : Controller
    {
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet]
        public IActionResult Private()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return BadRequest();
            }

            return Content($"You have authorized access to resources belonging to {identity.Name} on Zirku.Api1.");
        }

        [HttpGet]
        public IActionResult Public()
        {
            return Content("This is a public endpoint that is at Zirku.Api1; it does not require authorization.");
        }
    }
}
