using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Balosar.Server.Data;
using Balosar.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace Balosar.Server.Controllers;

// [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
// using the above Authorize will break the server side CLAIMS.
[ApiController, Route("/api/[controller]")]
public class ClaimsController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public ClaimsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpPost("add-sample")]
    [Authorize]
    public async Task<IActionResult> AddSampleClaims()
    {
        var userId = HttpContext.User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;

        if (await _dbContext.UserClaims.Where(m => m.UserId == userId).AnyAsync())
            return BadRequest("Already added");

        await _dbContext.AddAsync(new IdentityUserClaim<string>
        {
            ClaimType = "ENTITY_TYPE_1",
            ClaimValue = "VIEW",
            UserId = userId
        });
        await _dbContext.AddAsync(new IdentityUserClaim<string>
        {
            ClaimType = "ENTITY_TYPE_1",
            ClaimValue = "ADD",
            UserId = userId
        });
        await _dbContext.AddAsync(new IdentityUserClaim<string>
        {
            ClaimType = "ENTITY_TYPE_2",
            ClaimValue = "VIEW",
            UserId = userId
        });
        await _dbContext.SaveChangesAsync();
        return Ok();
    }


    [HttpGet("has-claim")]
    [Authorize]
    [ResponseCache(NoStore = true)]
    public IActionResult HasClaim([FromQuery]string type, [FromQuery]string value)
    {
        var claims = HttpContext.User.Claims.ToList();
        var hasClaim = HttpContext.User.HasClaim(type, value);
        return Ok(hasClaim);
    }
}