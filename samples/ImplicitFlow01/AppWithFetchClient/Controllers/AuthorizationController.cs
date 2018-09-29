using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server;

public class AuthorizationController : Controller
{
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly OpenIddictScopeManager<OpenIddictScope> _scopeManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthorizationController(
        IOptions<IdentityOptions> identityOptions,
        OpenIddictScopeManager<OpenIddictScope> scopeManager,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _identityOptions = identityOptions;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("~/connect/authorize")]
    public async Task<IActionResult> Authorize(OpenIdConnectRequest oidcRequest)
    {
        // This demo only supports first-party clients with prompt=none.
        if (!oidcRequest.HasPrompt(OpenIdConnectConstants.Prompts.None))
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIdConnectConstants.Properties.Error] = OpenIdConnectConstants.Errors.InvalidRequest,
                [OpenIdConnectConstants.Properties.ErrorDescription] 
                    = "The authorization request must have a prompt=none parameter."
            });

            // Ask OpenIddict to return an access_denied error to the client application.
            return Forbid(properties, OpenIddictServerDefaults.AuthenticationScheme);
        }

        if (!User.Identity.IsAuthenticated)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIdConnectConstants.Properties.Error] = OpenIdConnectConstants.Errors.LoginRequired,
                [OpenIdConnectConstants.Properties.ErrorDescription] = "The user is not logged in."
            });

            // Ask OpenIddict to return a login_required error to the client application.
            return Forbid(properties, OpenIddictServerDefaults.AuthenticationScheme);
        }

        // Retrieve the profile of the logged in user.
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIdConnectConstants.Properties.Error] = OpenIdConnectConstants.Errors.LoginRequired,
                [OpenIdConnectConstants.Properties.ErrorDescription] = "The user's account has been deleted."
            });

            // Ask OpenIddict to return a login_required error to the client application.
            return Forbid(properties, OpenIddictServerDefaults.AuthenticationScheme);
        }

        // Create a new authentication ticket.
        var ticket = await CreateTicketAsync(oidcRequest, user);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
    }

    private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest oidcRequest, IdentityUser user)
    {
        // Create a new ClaimsPrincipal containing the claims that
        // will be used to create an id_token, a token or a code.
        var principal = await _signInManager.CreateUserPrincipalAsync(user);

        // Create a new authentication ticket holding the user identity.
        var ticket = new AuthenticationTicket(principal,
            new AuthenticationProperties(),
            OpenIddictServerDefaults.AuthenticationScheme);

        // Set the list of scopes granted to the client application.
        var scopes = oidcRequest.GetScopes().ToImmutableArray();

        ticket.SetScopes(scopes);
        ticket.SetResources(await _scopeManager.ListResourcesAsync(scopes));

        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        foreach (var claim in ticket.Principal.Claims)
        {
            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
            {
                continue;
            }

            var destinations = new List<string>
            {
                OpenIdConnectConstants.Destinations.AccessToken
            };

            // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
            // The other claims will only be added to the access_token, which is encrypted when using the default format.
            if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
                (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
                (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
            {
                destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
            }

            claim.SetDestinations(destinations);
        }

        return ticket;
    }
}
