﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using OpenIddict.Client.Owin;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Mortis.Client.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet, Route("~/login")]
        public ActionResult LogIn(string returnUrl)
        {
            var context = HttpContext.GetOwinContext();

            var properties = new AuthenticationProperties
            {
                // Only allow local return URLs to prevent open redirect attacks.
                RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"
            };

            // Ask the OpenIddict client middleware to redirect the user agent to the identity provider.
            context.Authentication.Challenge(properties, OpenIddictClientOwinDefaults.AuthenticationType);
            return new EmptyResult();
        }

        [HttpPost, Route("~/logout"), ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOut(string returnUrl)
        {
            var context = HttpContext.GetOwinContext();

            // Retrieve the identity stored in the local authentication cookie. If it's not available,
            // this indicate that the user is already logged out locally (or has not logged in yet).
            var result = await context.Authentication.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationType);
            if (result is not { Identity: ClaimsIdentity })
            {
                // Only allow local return URLs to prevent open redirect attacks.
                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
            }

            // Remove the local authentication cookie before triggering a redirection to the remote server.
            context.Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);

            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                // While not required, the specification encourages sending an id_token_hint
                // parameter containing an identity token returned by the server for this user.
                [OpenIddictClientOwinConstants.Properties.IdentityTokenHint] =
                    result.Properties.Dictionary[OpenIddictClientOwinConstants.Tokens.BackchannelIdentityToken]
            })
            {
                // Only allow local return URLs to prevent open redirect attacks.
                RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"
            };

            // Ask the OpenIddict client middleware to redirect the user agent to the identity provider.
            context.Authentication.SignOut(properties, OpenIddictClientOwinDefaults.AuthenticationType);
            return Redirect(properties.RedirectUri);
        }

        // Note: this controller uses the same callback action for all providers
        // but for users who prefer using a different action per provider,
        // the following action can be split into separate actions.
        [AcceptVerbs("GET", "POST"), Route("~/callback/login/{provider}")]
        public async Task<ActionResult> LogInCallback()
        {
            var context = HttpContext.GetOwinContext();

            // Retrieve the authorization data validated by OpenIddict as part of the callback handling.
            var result = await context.Authentication.AuthenticateAsync(OpenIddictClientOwinDefaults.AuthenticationType);

            // Multiple strategies exist to handle OAuth 2.0/OpenID Connect callbacks, each with their pros and cons:
            //
            //   * Directly using the tokens to perform the necessary action(s) on behalf of the user, which is suitable
            //     for applications that don't need a long-term access to the user's resources or don't want to store
            //     access/refresh tokens in a database or in an authentication cookie (which has security implications).
            //     It is also suitable for applications that don't need to authenticate users but only need to perform
            //     action(s) on their behalf by making API calls using the access token returned by the remote server.
            //
            //   * Storing the external claims/tokens in a database (and optionally keeping the essential claims in an
            //     authentication cookie so that cookie size limits are not hit). For the applications that use ASP.NET
            //     Core Identity, the UserManager.SetAuthenticationTokenAsync() API can be used to store external tokens.
            //
            //     Note: in this case, it's recommended to use column encryption to protect the tokens in the database.
            //
            //   * Storing the external claims/tokens in an authentication cookie, which doesn't require having
            //     a user database but may be affected by the cookie size limits enforced by most browser vendors
            //     (e.g Safari for macOS and Safari for iOS/iPadOS enforce a per-domain 4KB limit for all cookies).
            //
            //     Note: this is the approach used here, but the external claims are first filtered to only persist
            //     a few claims like the user identifier. The same approach is used to store the access/refresh tokens.

            // Important: if the remote server doesn't support OpenID Connect and doesn't expose a userinfo endpoint,
            // result.Principal.Identity will represent an unauthenticated identity and won't contain any claim.
            //
            // Such identities cannot be used as-is to build an authentication cookie in ASP.NET (as the
            // antiforgery stack requires at least a name claim to bind CSRF cookies to the user's identity) but
            // the access/refresh tokens can be retrieved using result.Properties.GetTokens() to make API calls.
            if (result.Identity is not ClaimsIdentity { IsAuthenticated: true })
            {
                throw new InvalidOperationException("The external authorization data cannot be used for authentication.");
            }

            // Build an identity based on the external claims and that will be used to create the authentication cookie.
            //
            // By default, all claims extracted during the authorization dance are available. The claims collection stored
            // in the cookie can be filtered out or mapped to different names depending the claim name or its issuer.
            var claims = result.Identity.Claims.Where(claim => claim.Type is ClaimTypes.NameIdentifier or ClaimTypes.Name
                //
                // Preserve the registration details to be able to resolve them later.
                //
                or Claims.Private.RegistrationId or Claims.Private.ProviderName
                //
                // The ASP.NET 4.x antiforgery module requires preserving the "identityprovider" claim.
                //
                or "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider");

            var identity = new ClaimsIdentity(claims,
                authenticationType: CookieAuthenticationDefaults.AuthenticationType,
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            // Build the authentication properties based on the properties that were added when the challenge was triggered.
            var properties = new AuthenticationProperties(result.Properties.Dictionary
                .Where(item => item.Key is
                    // Preserve the return URL.
                    ".redirect" or

                    // If needed, the tokens returned by the authorization server can be stored in the authentication cookie.
                    OpenIddictClientOwinConstants.Tokens.BackchannelAccessToken   or
                    OpenIddictClientOwinConstants.Tokens.BackchannelIdentityToken or
                    OpenIddictClientOwinConstants.Tokens.RefreshToken)
                .ToDictionary(pair => pair.Key, pair => pair.Value));

            context.Authentication.SignIn(properties, identity);
            return Redirect(properties.RedirectUri ?? "/");
        }

        // Note: this controller uses the same callback action for all providers
        // but for users who prefer using a different action per provider,
        // the following action can be split into separate actions.
        [AcceptVerbs("GET", "POST"), Route("~/callback/logout/{provider}")]
        public async Task<ActionResult> LogOutCallback()
        {
            var context = HttpContext.GetOwinContext();

            // Retrieve the data stored by OpenIddict in the state token created when the logout was triggered.
            var result = await context.Authentication.AuthenticateAsync(OpenIddictClientOwinDefaults.AuthenticationType);

            // In this sample, the local authentication cookie is always removed before the user agent is redirected
            // to the authorization server. Applications that prefer delaying the removal of the local cookie can
            // remove the corresponding code from the logout action and remove the authentication cookie in this action.

            return Redirect(result!.Properties!.RedirectUri);
        }
    }
}
