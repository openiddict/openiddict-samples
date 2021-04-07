using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Validation.AspNetCore;
using Ralltiir.Server.Models;

namespace Ralltiir.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResourceController : Controller
    {
        private readonly OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>> _applicationManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ResourceController(
            OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>> applicationManager,
            UserManager<ApplicationUser> userManager)
        {
            _applicationManager = applicationManager;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("message")]
        public async Task<IActionResult> GetMessage()
        {
            var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
            if (subject == null) return NotFound();
            
            var user = await _userManager.FindByIdAsync(subject);
            
            return Content($"{user.Email} has been successfully authenticated.");
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
            Roles = "moderator")]
        [HttpGet("message/moderator")]
        public async Task<IActionResult> GetGroup1Message()
        {
            var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
            if (subject == null) return NotFound();
            
            var user = await _userManager.FindByIdAsync(subject);
            
            return Content($"{user.Email} has been granted moderator role.");
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
            Roles = "admin")]
        [HttpGet("message/admin")]
        public async Task<IActionResult> GetAdminMessage()
        {
            var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
            if (subject == null) return NotFound();
            
            var user = await _userManager.FindByIdAsync(subject);
            
            return Content($"{user.Email} has been granted admin role.");
        }
    }
}