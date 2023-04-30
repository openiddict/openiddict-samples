using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Dantooine.WebAssembly.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Dantooine.WebAssembly.Server.Controllers;

// Original source: https://github.com/berhir/BlazorWebAssemblyCookieAuth.
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetCurrentUser()
    {
        return Ok(User.Identity.IsAuthenticated ? CreateUserInfo(User) : UserInfo.Anonymous);
    }

    private static UserInfo CreateUserInfo(ClaimsPrincipal principal)
    {
        if (!principal.Identity.IsAuthenticated)
        {
            return UserInfo.Anonymous;
        }

        var userinfo = new UserInfo
        {
            IsAuthenticated = true
        };

        if (principal.Identity is ClaimsIdentity identity)
        {
            userinfo.NameClaimType = identity.NameClaimType;
            userinfo.RoleClaimType = identity.RoleClaimType;
        }
        else
        {
            userinfo.NameClaimType = Claims.Name;
            userinfo.RoleClaimType = Claims.Role;
        }

        if (principal.Claims.Any())
        {
            var claims = new List<ClaimValue>();

            foreach (var claim in principal.FindAll(userinfo.NameClaimType))
            {
                claims.Add(new ClaimValue(userinfo.NameClaimType, claim.Value));
            }

            userinfo.Claims = claims;
        }

        return userinfo;
    }
}
