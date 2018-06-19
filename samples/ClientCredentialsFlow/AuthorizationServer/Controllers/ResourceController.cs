using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Validation;

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

        [Authorize(ActiveAuthenticationSchemes = OpenIddictValidationDefaults.AuthenticationScheme)]
        [HttpGet("message")]
        public async Task<IActionResult> GetMessage()
        {
            var subject = User.FindFirst(OpenIdConnectConstants.Claims.Subject)?.Value;
            if (string.IsNullOrEmpty(subject))
            {
                return BadRequest();
            }

            var application = await _applicationManager.FindByClientIdAsync(subject, HttpContext.RequestAborted);
            if (application == null)
            {
                return BadRequest();
            }

            return Content($"{application.DisplayName} has been successfully authenticated.");
        }
    }
}