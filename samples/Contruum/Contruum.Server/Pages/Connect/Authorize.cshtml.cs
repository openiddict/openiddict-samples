using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Contruum.Server.Pages.Connect;

public class AuthorizeModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest()!;

        // Retrieve the user principal stored in the authentication cookie.
        // If a max_age parameter was provided, ensure that the cookie is not too old.
        // If the user principal can't be extracted or the cookie is too old, redirect the user to the login page.
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (result == null || !result.Succeeded || (request.MaxAge != null && result.Properties?.IssuedUtc != null &&
            DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
        {
            // If the client application requested promptless authentication,
            // return an error indicating that the user is not logged in.
            if (request.HasPrompt(Prompts.None))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    }));
            }

            return Challenge(
                authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // If prompt=login was specified by the client application,
        // immediately return the user agent to the login page.
        if (request.HasPrompt(Prompts.Login))
        {
            // To avoid endless login -> authorization redirects, the prompt=login flag
            // is removed from the authorization request payload before redirecting the user.
            var prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));

            var parameters = Request.HasFormContentType ?
                Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList() :
                Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();

            parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

            return Challenge(
                authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
                });
        }

        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        var identity = new ClaimsIdentity(result.Principal!.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // The OP-Req-acr_values test consists in sending an "acr_values=1 2" parameter
        // as part of the authorization request. To indicate to the certification client
        // that the "1" reference value was satisfied, an "acr" claim is added.
        if (request.HasAcrValue("1"))
        {
            identity.AddClaim(new Claim(Claims.AuthenticationContextReference, "1"));
        }

        identity.SetScopes(new[]
        {
            Scopes.OfflineAccess,
            Scopes.OpenId,
            Scopes.Address,
            Scopes.Email,
            Scopes.Phone,
            Scopes.Profile
        }.Intersect(request.GetScopes()));

        identity.SetDestinations(claim => claim.Type switch
        {
            // Note: always include acr and auth_time in the identity token as they must be flowed
            // from the authorization endpoint to the identity token returned from the token endpoint.
            Claims.AuthenticationContextReference or
            Claims.AuthenticationTime
                => ImmutableArray.Create(Destinations.IdentityToken),

            // Note: when an authorization code or access token is returned, don't add the profile, email,
            // phone and address claims to the identity tokens as they are returned from the userinfo endpoint.
            Claims.Subject or
            Claims.Name or
            Claims.Gender or
            Claims.GivenName or
            Claims.MiddleName or
            Claims.FamilyName or
            Claims.Nickname or
            Claims.PreferredUsername or
            Claims.Birthdate or
            Claims.Profile or
            Claims.Picture or
            Claims.Website or
            Claims.Locale or
            Claims.Zoneinfo or
            Claims.UpdatedAt when identity.HasScope(Scopes.Profile) &&
                !request.HasResponseType(ResponseTypes.Code) &&
                !request.HasResponseType(ResponseTypes.Token)
                => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

            Claims.Email when identity.HasScope(Scopes.Email) &&
                !request.HasResponseType(ResponseTypes.Code) &&
                !request.HasResponseType(ResponseTypes.Token)
                => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

            Claims.PhoneNumber when identity.HasScope(Scopes.Phone) &&
                !request.HasResponseType(ResponseTypes.Code) &&
                !request.HasResponseType(ResponseTypes.Token)
                => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

            Claims.Address when identity.HasScope(Scopes.Address) &&
                !request.HasResponseType(ResponseTypes.Code) &&
                !request.HasResponseType(ResponseTypes.Token)
                => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

            _ => ImmutableArray.Create(Destinations.AccessToken)
        });

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
