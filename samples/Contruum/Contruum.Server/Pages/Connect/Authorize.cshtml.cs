using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Contruum.Server.Pages.Connect;

public class AuthorizeModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Try to retrieve the user principal stored in the authentication cookie and redirect
        // the user agent to the login page (or to an external provider) in the following cases:
        //
        //  - If the user principal can't be extracted or the cookie is too old.
        //  - If prompt=login was specified by the client application.
        //  - If max_age=0 was specified by the client application (max_age=0 is equivalent to prompt=login).
        //  - If a max_age parameter was provided and the authentication cookie is not considered "fresh" enough.
        //
        // For scenarios where the default authentication handler configured in the ASP.NET Core
        // authentication options shouldn't be used, a specific scheme can be specified here.
        var result = await HttpContext.AuthenticateAsync();
        if (result is not { Succeeded: true } ||
            ((request.HasPromptValue(PromptValues.Login) || request.MaxAge is 0 ||
             (request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
              TimeProvider.System.GetUtcNow() - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value))) &&
            TempData["IgnoreAuthenticationChallenge"] is null or false))
        {
            // If the client application requested promptless authentication,
            // return an error indicating that the user is not logged in.
            if (request.HasPromptValue(PromptValues.None))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    }));
            }

            // To avoid endless login endpoint -> authorization endpoint redirects, a special temp data entry is
            // used to skip the challenge if the user agent has already been redirected to the login endpoint.
            //
            // Note: this flag doesn't guarantee that the user has accepted to re-authenticate. If such a guarantee
            // is needed, the existing authentication cookie MUST be deleted AND revoked (e.g using ASP.NET Core
            // Identity's security stamp feature with an extremely short revalidation time span) before triggering
            // a challenge to redirect the user agent to the login endpoint.
            TempData["IgnoreAuthenticationChallenge"] = true;

            // For scenarios where the default challenge handler configured in the ASP.NET Core
            // authentication options shouldn't be used, a specific scheme can be specified here.
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                    Request.HasFormContentType ? Request.Form : Request.Query)
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
