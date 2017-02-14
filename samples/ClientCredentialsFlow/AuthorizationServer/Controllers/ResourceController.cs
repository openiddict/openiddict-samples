using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Core;
using OpenIddict.Models;

namespace AuthorizationServer.Controllers
{
    [Route("api")]
    public class ResourceController : Controller
    {
        private readonly OpenIddictApplicationManager<OpenIddictApplication> _applicationManager;

        public ResourceController(OpenIddictApplicationManager<OpenIddictApplication> applicationManager)
        {
            _applicationManager = applicationManager;
        }

        [Authorize(ActiveAuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
        [HttpGet("message")]
        public async Task<IActionResult> GetMessage()
        {
            var identifier = User.FindFirst(ClaimTypes.NameIdentifier);

            var application = await _applicationManager.FindByClientIdAsync(identifier.Value, HttpContext.RequestAborted);
            if (application == null)
            {
                return BadRequest();
            }

            return Content($"{application.DisplayName} has been successfully authenticated.");
        }
    }
}