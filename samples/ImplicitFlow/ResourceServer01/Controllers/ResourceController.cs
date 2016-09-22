using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ResourceServer01.Controllers
{
    public class ResourceController : Controller {
        [Authorize(ActiveAuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
        [HttpGet]
        public IActionResult Private() {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null) {
                return BadRequest();
            }

            return Content($"You have authorized access to resources belonging to {identity.Name} on ResourceServer01.");
        }

        [HttpGet]
        public IActionResult Public() {
            return Content("This is a public endpoint that is at ResourceServer01; it does not require authorization.");
        }    
    }
}
