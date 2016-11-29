using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using AspNet.Security.OpenIdConnect.Primitives;
using AuthorizationServer.Models;
using AuthorizationServer.ViewModels;
using AuthorizationServer.BindingModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace AuthorizationServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IExternalAuthorizationManager _externalAuthManager;
        private static bool _databaseChecked;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext applicationDbContext,
            IExternalAuthorizationManager externalAuthManager
            )
        {
            _externalAuthManager = externalAuthManager;
            _userManager = userManager;
            _applicationDbContext = applicationDbContext;
        }

        //
        // POST: /Account/Register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm]RegisterViewModel model)
        {
            EnsureDatabaseCreated(_applicationDbContext);
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.UserName };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return Ok();
                }
                AddErrors(result);
            }

            // If we got this far, something failed.
            return BadRequest(ModelState);
        }

        [HttpPost("RegisterExternal")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterExternal([FromBody]RegisterExternalBindingModel model)
        {
            EnsureDatabaseCreated(_applicationDbContext);
            if (ModelState.IsValid)
            {
                var isValid = await _externalAuthManager.VerifyExternalAccessToken(model.AccessToken, model.Provider);

                if (!isValid)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidRequest,
                        ErrorDescription = "Invalid access_token, this usually happens when it is expired"
                    });
                }

                var profile = await _externalAuthManager.GetProfile(model.AccessToken, model.Provider);

                var user = new ApplicationUser {
                    UserName = profile.email,
                    Email = profile.email,
                    FirstName = profile.first_name,
                    LastName = profile.last_name
                };
                var externalAccount = new ExternalAccount() {
                    Id = Guid.NewGuid().ToString(),
                    AddedAt = DateTimeOffset.Now,
                    Provider = model.Provider,
                    ProviderUserId = profile.id
                };
                user.ExternalAccounts.Add(externalAccount);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    return Ok();
                }
                AddErrors(result);
            }

            // If we got this far, something failed.
            return BadRequest(ModelState);
        }

        #region Helpers

        // The following code creates the database and schema if they don't exist.
        // This is a temporary workaround since deploying database through EF migrations is
        // not yet supported in this release.
        // Please see this http://go.microsoft.com/fwlink/?LinkID=615859 for more information on how to do deploy the database
        // when publishing your application.
        private static void EnsureDatabaseCreated(ApplicationDbContext context)
        {
            if (!_databaseChecked)
            {
                _databaseChecked = true;
                context.Database.EnsureCreated();
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        #endregion
    }
}
