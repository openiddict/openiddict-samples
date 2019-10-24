/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server;

namespace AuthorizationServer.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly OpenIddictApplicationManager<OpenIddictApplication> _applicationManager;

        public AuthorizationController(OpenIddictApplicationManager<OpenIddictApplication> applicationManager)
        {
            _applicationManager = applicationManager;
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIdConnectRequest();
            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);
                if (application == null)
                {
                    throw new InvalidOperationException("The application details cannot be found in the database.");
                }

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token, a token or a code.
                var identity = new ClaimsIdentity(
                    OpenIddictServerDefaults.AuthenticationScheme,
                    OpenIdConnectConstants.Claims.Name,
                    OpenIdConnectConstants.Claims.Role);

                // Use the client_id as the subject identifier.
                identity.AddClaim(OpenIdConnectConstants.Claims.Subject, application.ClientId,
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);

                identity.AddClaim(OpenIdConnectConstants.Claims.Name, application.DisplayName,
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);

                // Create a new authentication ticket holding the user identity.
                var ticket = new AuthenticationTicket(
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties(),
                    OpenIddictServerDefaults.AuthenticationScheme);

                ticket.SetResources("resource_server");

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }
    }
}