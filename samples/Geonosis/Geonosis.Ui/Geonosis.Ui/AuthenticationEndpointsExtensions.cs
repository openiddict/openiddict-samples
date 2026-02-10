using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Geonosis.Ui
{
    /// <summary>
    /// Extension methods for mapping authentication-related endpoints including login, logout, and their callbacks.
    /// </summary>
    internal static class AuthenticationEndpointsExtensions
    {
        /// <summary>
        /// Maps all authentication endpoints: login, logout, and their OpenID Connect callbacks.
        /// </summary>
        internal static RouteGroupBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder routes)
        {
            var authGroup = routes.MapGroup("/authentication");

            RegisterLoginEndpoint(authGroup);
            RegisterLogoutEndpoint(authGroup);
            RegisterCallbackEndpoints(authGroup);

            return authGroup;
        }

        private static void RegisterLoginEndpoint(RouteGroupBuilder authGroup)
        {
            authGroup.MapGet("/login", (string? returnUrl) =>
            {
                return TypedResults.Challenge(
                    BuildRedirectProperties(returnUrl),
                    [
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        OpenIddictClientAspNetCoreDefaults.AuthenticationScheme
                    ]);
            }).AllowAnonymous();
        }

        private static void RegisterLogoutEndpoint(RouteGroupBuilder authGroup)
        {
            authGroup.MapPost("/logout", ([FromForm] string? returnUrl, HttpContext ctx, IAntiforgery antiforgery) =>
            {
                return TypedResults.SignOut(
                    BuildRedirectProperties(returnUrl),
                    [
                        CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIddictClientAspNetCoreDefaults.AuthenticationScheme
                    ]);
            });
        }

        private static void RegisterCallbackEndpoints(RouteGroupBuilder authGroup)
        {
            authGroup.MapMethods(
                "/login-callback/{provider}",
                [HttpMethod.Get.Method, HttpMethod.Post.Method],
                async (string? provider, HttpContext ctx) => await HandleLoginCallback(ctx))
                .DisableAntiforgery();

            authGroup.MapMethods(
                "/logout-callback/{provider}",
                [HttpMethod.Get.Method, HttpMethod.Post.Method],
                async (string? provider, HttpContext ctx) => await HandleLogoutCallback(ctx))
                .DisableAntiforgery();
        }

        private static AuthenticationProperties BuildRedirectProperties(string? returnUrl)
        {
            const string baseRoute = "/";

            // Sanitize and validate return URL to prevent open redirects
            var sanitizedUrl = returnUrl switch
            {
                null or "" => baseRoute,
                _ when !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) =>
                    new Uri(returnUrl, UriKind.Absolute).PathAndQuery,
                _ when returnUrl[0] != '/' => $"{baseRoute}{returnUrl}",
                _ => returnUrl
            };

            return new AuthenticationProperties { RedirectUri = sanitizedUrl };
        }

        private static async Task<IResult> HandleLoginCallback(HttpContext ctx)
        {
            var authResult = await ctx.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

            if (authResult is not { Succeeded: true, Principal.Identity.IsAuthenticated: true })
            {
                throw new InvalidOperationException("External authorization failed or user is not authenticated.");
            }

            var userIdentity = new ClaimsIdentity(
                authenticationType: "ExternalLogin",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            // Map essential user claims from the external provider
            userIdentity.SetClaim(ClaimTypes.Email, authResult.Principal.GetClaim(ClaimTypes.Email))
                        .SetClaim(ClaimTypes.Name, authResult.Principal.GetClaim(ClaimTypes.Name))
                        .SetClaim(ClaimTypes.NameIdentifier, authResult.Principal.GetClaim(ClaimTypes.NameIdentifier))
                        .SetClaim(ClaimTypes.Role, authResult.Principal.GetClaim("Role"));

            // Store provider registration details
            userIdentity.SetClaim(Claims.Private.RegistrationId, authResult.Principal.GetClaim(Claims.Private.RegistrationId))
                        .SetClaim(Claims.Private.ProviderName, authResult.Principal.GetClaim(Claims.Private.ProviderName));

            var authProps = new AuthenticationProperties(authResult.Properties.Items)
            {
                RedirectUri = authResult.Properties.RedirectUri ?? "/",
                IssuedUtc = null,
                ExpiresUtc = null,
                IsPersistent = true
            };

            // Filter and store only necessary tokens
            authProps.StoreTokens(authResult.Properties.GetTokens().Where(t => t.Name is
                OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken or
                OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessTokenExpirationDate or
                OpenIddictClientAspNetCoreConstants.Tokens.BackchannelIdentityToken or
                OpenIddictClientAspNetCoreConstants.Tokens.RefreshToken));

            return TypedResults.SignIn(new ClaimsPrincipal(userIdentity), authProps);
        }

        private static async Task<IResult> HandleLogoutCallback(HttpContext ctx)
        {
            var authResult = await ctx.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
            var redirectTarget = authResult?.Properties?.RedirectUri ?? "/";

            return TypedResults.Redirect(redirectTarget);
        }
    }
}
