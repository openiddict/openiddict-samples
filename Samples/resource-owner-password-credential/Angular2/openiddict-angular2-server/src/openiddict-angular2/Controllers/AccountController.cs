using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using openiddict_angular2.Models;
using openiddict_angular2.Data;
using openiddict_angular2.Models.AccountViewModels;


namespace openiddict_angular2.Controllers
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
                                    ApplicationDbContext applicationDbContext
                                 )
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _logger = loggerFactory.CreateLogger<AccountController>();
            }


        // Custom Register
        [Route("api/account/register")]
        [HttpPost]
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
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation(3, "User created a new account with password.");
            return Ok(result);
        }



        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            returnUrl = returnUrl.Replace("#", "");
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {

                return Redirect(returnUrl);
            }
        }

        #endregion
    }
}
