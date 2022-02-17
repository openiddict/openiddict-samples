using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using Fornax.Server.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OpenIddict.Abstractions;
using OpenIddict.Server.Owin;
using Owin;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Fornax.Server.Connect
{
    public partial class Authorize : Page
    {
        // Note: these properties are automatically injected by Autofac.
        public IOpenIddictApplicationManager ApplicationManager { get; set; }
        public IOpenIddictAuthorizationManager AuthorizationManager { get; set; }
        public IOpenIddictScopeManager ScopeManager { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Parameters { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var context = Context.GetOwinContext();
                var request = context.GetOpenIddictServerRequest() ??
                    throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

                // Retrieve the user principal stored in the authentication cookie.
                // If a max_age parameter was provided, ensure that the cookie is not too old.
                // If the user principal can't be extracted or the cookie is too old, redirect the user to the login page.
                var result = await context.Authentication.AuthenticateAsync(DefaultAuthenticationTypes.ApplicationCookie);
                if (result == null || result.Identity == null || (request.MaxAge != null && result.Properties?.IssuedUtc != null &&
                    DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
                {
                    context.Authentication.Challenge(DefaultAuthenticationTypes.ApplicationCookie);
                    Visible = false;
                    return;
                }

                // Retrieve the profile of the logged in user.
                var user = await context.GetUserManager<ApplicationUserManager>().FindByIdAsync(result.Identity.GetUserId()) ??
                    throw new InvalidOperationException("The user details cannot be retrieved.");

                // Retrieve the application details from the database.
                var application = await ApplicationManager.FindByClientIdAsync(request.ClientId) ??
                    throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

                // Retrieve the permanent authorizations associated with the user and the calling client application.
                var authorizations = await AuthorizationManager.FindAsync(
                    subject: user.Id,
                    client : await ApplicationManager.GetIdAsync(application),
                    status : Statuses.Valid,
                    type   : AuthorizationTypes.Permanent,
                    scopes : request.GetScopes()).ToListAsync();

                switch (await ApplicationManager.GetConsentTypeAsync(application))
                {
                    // If the consent is external (e.g when authorizations are granted by a sysadmin),
                    // immediately return an error if no authorization can be found in the database.
                    case ConsentTypes.External when !authorizations.Any():
                        context.Authentication.Challenge(
                            authenticationTypes: OpenIddictServerOwinDefaults.AuthenticationType,
                            properties: new AuthenticationProperties(new Dictionary<string, string>
                            {
                                [OpenIddictServerOwinConstants.Properties.Error] = Errors.ConsentRequired,
                                [OpenIddictServerOwinConstants.Properties.ErrorDescription] =
                                    "The logged in user is not allowed to access this client application."
                            }));
                        Visible = false;
                        return;

                    // If the consent is implicit or if an authorization was found,
                    // return an authorization response without displaying the consent form.
                    case ConsentTypes.Implicit:
                    case ConsentTypes.External when authorizations.Any():
                    case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
                        var identity = new ClaimsIdentity(OpenIddictServerOwinDefaults.AuthenticationType);
                        identity.AddClaims((await context.Get<ApplicationSignInManager>().CreateUserIdentityAsync(user)).Claims);

                        identity.AddClaim(new Claim(Claims.Subject, identity.FindFirstValue(ClaimTypes.NameIdentifier)));
                        identity.AddClaim(new Claim(Claims.Name, identity.FindFirstValue(ClaimTypes.Name)));

                        var principal = new ClaimsPrincipal(identity);

                        // Note: in this sample, the granted scopes match the requested scope
                        // but you may want to allow the user to uncheck specific scopes.
                        // For that, simply restrict the list of scopes before calling SetScopes.
                        principal.SetScopes(request.GetScopes());
                        principal.SetResources(await ScopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

                        // Automatically create a permanent authorization to avoid requiring explicit consent
                        // for future authorization or token requests containing the same scopes.
                        var authorization = authorizations.LastOrDefault();
                        if (authorization == null)
                        {
                            authorization = await AuthorizationManager.CreateAsync(
                                principal: principal,
                                subject  : user.Id,
                                client   : await ApplicationManager.GetIdAsync(application),
                                type     : AuthorizationTypes.Permanent,
                                scopes   : principal.GetScopes());
                        }

                        principal.SetAuthorizationId(await AuthorizationManager.GetIdAsync(authorization));

                        foreach (var claim in principal.Claims)
                        {
                            claim.SetDestinations(GetDestinations(claim, principal));
                        }

                        context.Authentication.SignIn((ClaimsIdentity) principal.Identity);
                        Visible = false;
                        return;

                    // At this point, no authorization was found in the database and an error must be returned
                    // if the client application specified prompt=none in the authorization request.
                    case ConsentTypes.Explicit   when request.HasPrompt(Prompts.None):
                    case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
                        context.Authentication.Challenge(
                            authenticationTypes: OpenIddictServerOwinDefaults.AuthenticationType,
                            properties: new AuthenticationProperties(new Dictionary<string, string>
                            {
                                [OpenIddictServerOwinConstants.Properties.Error] = Errors.ConsentRequired,
                                [OpenIddictServerOwinConstants.Properties.ErrorDescription] =
                                    "Interactive user consent is required."
                            }));
                        Visible = false;
                        return;

                    // In every other case, render the consent form.
                    default:
                        ApplicationName.Text = await ApplicationManager.GetLocalizedDisplayNameAsync(application);
                        Scope.Text = request.Scope;

                        // Flow the request parameters so they can be received by the Accept/Reject actions.
                        Parameters = string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) ?
                            from name in Request.Form.AllKeys
                            from value in Request.Form.GetValues(name)
                            select new KeyValuePair<string, string>(name, value) :
                            from name in Request.QueryString.AllKeys
                            from value in Request.QueryString.GetValues(name)
                            select new KeyValuePair<string, string>(name, value);
                        return;
                }
            }));
        }

        // Important: this endpoint MUST be protected CSRF and session fixation attacks.
        // In applications generated using a Visual Studio 2012 or higher, antiforgery
        // is implemented at the master page level, in the Site.Master.cs file.
        protected void Accept(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var context = Context.GetOwinContext();
                var request = context.GetOpenIddictServerRequest() ??
                    throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

                // Retrieve the user principal stored in the authentication cookie.
                var result = await context.Authentication.AuthenticateAsync(DefaultAuthenticationTypes.ApplicationCookie);
                if (result == null || result.Identity == null)
                {
                    context.Authentication.Challenge(DefaultAuthenticationTypes.ApplicationCookie);
                    Visible = false;
                    return;
                }

                // Retrieve the profile of the logged in user.
                var user = await context.GetUserManager<ApplicationUserManager>().FindByIdAsync(result.Identity.GetUserId()) ??
                    throw new InvalidOperationException("The user details cannot be retrieved.");

                // Retrieve the application details from the database.
                var application = await ApplicationManager.FindByClientIdAsync(request.ClientId) ??
                    throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

                // Retrieve the permanent authorizations associated with the user and the calling client application.
                var authorizations = await AuthorizationManager.FindAsync(
                    subject: user.Id,
                    client : await ApplicationManager.GetIdAsync(application),
                    status : Statuses.Valid,
                    type   : AuthorizationTypes.Permanent,
                    scopes : request.GetScopes()).ToListAsync();

                // Note: the same check is already made in the other action but is repeated
                // here to ensure a malicious user can't abuse this POST-only endpoint and
                // force it to return a valid response without the external authorization.
                if (!authorizations.Any() && await ApplicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
                {
                    context.Authentication.Challenge(
                        authenticationTypes: OpenIddictServerOwinDefaults.AuthenticationType,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerOwinConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerOwinConstants.Properties.ErrorDescription] =
                                "The logged in user is not allowed to access this client application."
                        }));
                    Visible = false;
                    return;
                }

                var identity = new ClaimsIdentity(OpenIddictServerOwinDefaults.AuthenticationType);
                identity.AddClaims((await context.Get<ApplicationSignInManager>().CreateUserIdentityAsync(user)).Claims);

                identity.AddClaim(new Claim(Claims.Subject, identity.FindFirstValue(ClaimTypes.NameIdentifier)));
                identity.AddClaim(new Claim(Claims.Name, identity.FindFirstValue(ClaimTypes.Name)));

                var principal = new ClaimsPrincipal(identity);

                // Note: in this sample, the granted scopes match the requested scope
                // but you may want to allow the user to uncheck specific scopes.
                // For that, simply restrict the list of scopes before calling SetScopes.
                principal.SetScopes(request.GetScopes());
                principal.SetResources(await ScopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

                // Automatically create a permanent authorization to avoid requiring explicit consent
                // for future authorization or token requests containing the same scopes.
                var authorization = authorizations.LastOrDefault();
                if (authorization == null)
                {
                    authorization = await AuthorizationManager.CreateAsync(
                        principal: principal,
                        subject  : user.Id,
                        client   : await ApplicationManager.GetIdAsync(application),
                        type     : AuthorizationTypes.Permanent,
                        scopes   : principal.GetScopes());
                }

                principal.SetAuthorizationId(await AuthorizationManager.GetIdAsync(authorization));

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinations(claim, principal));
                }

                context.Authentication.SignIn((ClaimsIdentity) principal.Identity);
                Visible = false;
                return;
            }));
        }

        protected void Deny(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                return;
            }

            // Notify OpenIddict that the authorization grant has been denied by the resource owner
            // to redirect the user agent to the client application using the appropriate response_mode.
            var context = Context.GetOwinContext();
            context.Authentication.Challenge(OpenIddictServerOwinDefaults.AuthenticationType);
            Visible = false;
        }

        private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case Constants.DefaultSecurityStampClaimType: yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}