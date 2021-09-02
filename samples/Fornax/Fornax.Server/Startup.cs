using Microsoft.Owin;
using OpenIddict.Server.Owin;
using OpenIddict.Validation.Owin;
using Owin;

[assembly: OwinStartup(typeof(Fornax.Server.Startup))]

namespace Fornax.Server
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Register the Autofac scope injector middleware.
            app.UseAutofacLifetimeScopeInjector(Global.Provider.ApplicationContainer);

            // Register the two OpenIddict server/validation middleware.
            app.UseMiddlewareFromContainer<OpenIddictServerOwinMiddleware>();
            app.UseMiddlewareFromContainer<OpenIddictValidationOwinMiddleware>();
        }
    }
}
