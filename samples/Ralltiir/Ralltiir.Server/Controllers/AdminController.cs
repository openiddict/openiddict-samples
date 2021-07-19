using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Ralltiir.Server.Models;
using Ralltiir.Server.ViewModels;

namespace Ralltiir.Server.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
        Roles = "admin")]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationUserRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationUserRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
            Roles = "admin,moderator")]
        [HttpGet("users")]
        public async Task<IEnumerable<UserViewModel>> GetUsers()
        {
            var usersTasks = (await _userManager.Users.ToListAsync())
                .Select(async user => new UserViewModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Roles = await _userManager.GetRolesAsync(user)
                });

            var result = await Task.WhenAll(usersTasks);

            return result;
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
            Roles = "admin,moderator")]
        [HttpGet("roles")]
        public async Task<IEnumerable<RoleViewModel>> GetRoles()
        {
            var result = (await _roleManager.Roles.ToListAsync())
                .Select(role => new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    DisplayName = role.NormalizedName
                });

            return result;
        }

        [HttpPost("users/{userId}/roles/{roleName}")]
        public async Task GrantUserRole(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            await _userManager.AddToRoleAsync(user, roleName);
        }

        [HttpDelete("users/{userId}/roles/{roleName}")]
        public async Task RevokeUserRole(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            await _userManager.RemoveFromRoleAsync(user, roleName);
        }
    }
}
