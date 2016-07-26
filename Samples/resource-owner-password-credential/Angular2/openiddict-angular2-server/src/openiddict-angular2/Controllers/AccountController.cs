using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NgOidc.Models;
using NgOidc.Data;
using NgOidc.Models.AccountViewModels;
using System;

namespace NgOidc.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory,
            ApplicationDbContext applicationDbContext)
        {
          _userManager = userManager;
          _signInManager = signInManager;
          _logger = loggerFactory.CreateLogger<AccountController>();
        }
        // Custom Register
        [Route("api/account/register")]
        [HttpPost]
      //  [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterHttp([FromBody]RegisterViewModel dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //currently Email and Username are same 
            var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation(3, "User created a new account with password.");
                return Ok(result);
            }

            return BadRequest(result);  
        }

        #region Helpers
        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        #endregion
    }
}
