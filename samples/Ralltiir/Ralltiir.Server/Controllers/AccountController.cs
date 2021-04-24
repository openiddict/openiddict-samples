using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ralltiir.Server.Models;
using Ralltiir.Server.ViewModels;

namespace Ralltiir.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            var user = new ApplicationUser {UserName = request.Email, Email = request.Email};
            
            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (result.Succeeded)
            {
                return Ok();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost("~/connect/login")]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe,
                lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return Ok();
            }

            if (result.RequiresTwoFactor)
            {
                // return RedirectToAction(nameof(SendCode), new {ReturnUrl = returnUrl, RememberMe = model.RememberMe});
                throw new NotImplementedException();
            }

            if (result.IsLockedOut)
            {
                return Forbid("Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return BadRequest(ModelState);
        }
        
        //
        // POST: /Account/LogOff
        [Authorize, HttpPost("~/connect/logout"), ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
