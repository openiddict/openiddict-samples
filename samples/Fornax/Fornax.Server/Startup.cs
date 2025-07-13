using Autofac;
using Fornax.Server.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using OpenIddict.Server.Owin;
using OpenIddict.Validation.Owin;
using Owin;

[assembly: OwinStartup(typeof(Fornax.Server.Startup))]

namespace Fornax.Server;

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        // Register the Entity Framework context and the user/sign-in managers used by ASP.NET Identity.
        //
        // Note: while the regular CreatePerOwinContext() method is used to store the Entity Framework
        // context in the OWIN environment (so that it can be retrieved by the ASP.NET Identity components),
        // the factory used here simply resolves the scoped context from the Autofac container to avoid
        // having multiple instances of the same context created for each request. As such, the dispose
        // callback action is left empty, as the context will be disposed by Autofac when the request ends.
        app.CreatePerOwinContext<ApplicationDbContext>(
            createCallback: static (options, context) => Global.Provider.ApplicationContainer.Resolve<ApplicationDbContext>(),
            disposeCallback: static (options, context) => { });

        app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
        app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

        // Register the cookie middleware used by ASP.NET Identity.
        app.UseCookieAuthentication(new CookieAuthenticationOptions
        {
            AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
            LoginPath = new PathString("/Account/Login"),
            Provider = new CookieAuthenticationProvider
            {
                OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                    validateInterval: TimeSpan.FromMinutes(30),
                    regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
            }
        });

        app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
        app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

        // Register the Autofac scope injector middleware.
        app.UseAutofacLifetimeScopeInjector(Global.Provider.ApplicationContainer);

        // Register the two OpenIddict server/validation middleware.
        app.UseMiddlewareFromContainer<OpenIddictServerOwinMiddleware>();
        app.UseMiddlewareFromContainer<OpenIddictValidationOwinMiddleware>();
    }
}
