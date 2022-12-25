using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security.Cookies;
using Mortis.Client.Models;
using OpenIddict.Client;
using OpenIddict.Client.Owin;
using Owin;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Mortis.Client
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var services = new ServiceCollection();

            services.AddOpenIddict()

                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework 6.x stores and models.
                    // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                    options.UseEntityFramework()
                           .UseDbContext<ApplicationDbContext>();

                    // Developers who prefer using MongoDB can remove the previous lines
                    // and configure OpenIddict to use the specified MongoDB database:
                    // options.UseMongoDb()
                    //        .UseDatabase(new MongoClient().GetDatabase("openiddict"));
                })

                // Register the OpenIddict client components.
                .AddClient(options =>
                {
                    // Note: this sample uses the code flow, but you can enable the other flows if necessary.
                    options.AllowAuthorizationCodeFlow();

                    // Register the signing and encryption credentials used to protect
                    // sensitive data like the state tokens produced by OpenIddict.
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    // Register the OWIN host and configure the OWIN-specific options.
                    options.UseOwin()
                           .EnableRedirectionEndpointPassthrough()
                           .EnablePostLogoutRedirectionEndpointPassthrough()
                           .SetCookieManager(new SystemWebCookieManager());

                    // Register the System.Net.Http integration and use the identity of the current
                    // assembly as a more specific user agent, which can be useful when dealing with
                    // providers that use the user agent as a way to throttle requests (e.g Reddit).
                    options.UseSystemNetHttp()
                           .SetProductInformation(typeof(Startup).Assembly);

                    // Add a client registration matching the client application definition in the server project.
                    options.AddRegistration(new OpenIddictClientRegistration
                    {
                        Issuer = new Uri("https://localhost:44349/", UriKind.Absolute),

                        ClientId = "mvc",
                        ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                        Scopes = { Scopes.Email, Scopes.Profile },

                        // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
                        // URI per provider, unless all the registered providers support returning a special "iss"
                        // parameter containing their URL as part of authorization responses. For more information,
                        // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
                        RedirectUri = new Uri("callback/login/local", UriKind.Relative),
                        PostLogoutRedirectUri = new Uri("callback/logout/local", UriKind.Relative)
                    });
                });

            // Create a new Autofac container and import the OpenIddict services.
            var builder = new ContainerBuilder();
            builder.Populate(services);

            // Register the MVC controllers.
            builder.RegisterControllers(typeof(Startup).Assembly);

            var container = builder.Build();

            // Register the Autofac scope injector middleware.
            app.UseAutofacLifetimeScopeInjector(container);

            // Register the cookie middleware responsible for storing the user sessions.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                ExpireTimeSpan = TimeSpan.FromMinutes(50),
                SlidingExpiration = false
            });

            // Register the OpenIddict middleware.
            app.UseMiddlewareFromContainer<OpenIddictClientOwinMiddleware>();

            // Configure ASP.NET MVC 5.2 to use Autofac when activating controller instances.
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Create the database used by the OpenIddict client stack to store tokens.
            // Note: in a real world application, this step should be part of a setup script.
            Task.Run(async delegate
            {
                await using var scope = container.BeginLifetimeScope();

                var context = scope.Resolve<ApplicationDbContext>();
                context.Database.CreateIfNotExists();
            }).GetAwaiter().GetResult();
        }
    }
}
