/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Aridka.Server.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> _applicationManager;

        public AuthorizationController(OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> applicationManager)
            => _applicationManager = applicationManager;

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
                if (application == null)
                {
                    throw new InvalidOperationException("The application details cannot be found in the database.");
                }

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token, a token or a code.
                var identity = new ClaimsIdentity(
                    TokenValidationParameters.DefaultAuthenticationType,
                    Claims.Name, Claims.Role);

                // Use the client_id as the subject identifier.
                identity.AddClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application),
                    Destinations.AccessToken, Destinations.IdentityToken);

                identity.AddClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application),
                    Destinations.AccessToken, Destinations.IdentityToken);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }
    }
}