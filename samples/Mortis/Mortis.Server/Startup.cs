using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Mortis.Server.Models;
using OpenIddict.Abstractions;
using OpenIddict.Server.Owin;
using OpenIddict.Validation.Owin;
using Owin;
using static OpenIddict.Abstractions.OpenIddictConstants;

[assembly: OwinStartup(typeof(Mortis.Server.Startup))]
namespace Mortis.Server
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
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    // Enable the authorization, logout and token endpoints.
                    options.SetAuthorizationEndpointUris("connect/authorize")
                           .SetLogoutEndpointUris("connect/logout")
                           .SetTokenEndpointUris("connect/token");

                    // Mark the "email", "profile" and "roles" scopes as supported scopes.
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                    // Note: this sample only uses the authorization code flow but you can enable
                    // the other flows if you need to support implicit, password or client credentials.
                    options.AllowAuthorizationCodeFlow();

                    // Register the signing and encryption credentials.
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    // Register the OWIN host and configure the OWIN-specific options.
                    options.UseOwin()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableLogoutEndpointPassthrough()
                           .EnableTokenEndpointPassthrough();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the OWIN host.
                    options.UseOwin();
                });

            // Create a new Autofac container and import the OpenIddict services.
            var builder = new ContainerBuilder();
            builder.Populate(services);

            // Register the MVC controllers.
            builder.RegisterControllers(typeof(Startup).Assembly);

            // Register the Web API controllers.
            builder.RegisterApiControllers(typeof(Startup).Assembly);

            var container = builder.Build();

            ConfigureAuth(app);

            // Register the Autofac scope injector middleware.
            app.UseAutofacLifetimeScopeInjector(container);

            // Register the two OpenIddict server/validation middleware.
            app.UseMiddlewareFromContainer<OpenIddictServerOwinMiddleware>();
            app.UseMiddlewareFromContainer<OpenIddictValidationOwinMiddleware>();

            // Configure ASP.NET MVC 5.2 to use Autofac when activating controller instances.
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Configure ASP.NET MVC 5.2 to use Autofac when activating controller instances
            // and infer the Web API routes using the HTTP attributes used in the controllers.
            var configuration = new HttpConfiguration
            {
                DependencyResolver = new AutofacWebApiDependencyResolver(container)
            };

            configuration.MapHttpAttributeRoutes();

            // Register the Autofac Web API integration and Web API middleware.
            app.UseAutofacWebApi(configuration);
            app.UseWebApi(configuration);

            // Seed the database with the sample client using the OpenIddict application manager.
            // Note: in a real world application, this step should be part of a setup script.
            Task.Run(async delegate
            {
                await using var scope = container.BeginLifetimeScope();

                var context = scope.Resolve<ApplicationDbContext>();
                context.Database.CreateIfNotExists();

                var manager = scope.Resolve<IOpenIddictApplicationManager>();

                if (await manager.FindByClientIdAsync("mvc") == null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "mvc",
                        ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "MVC client application",
                        RedirectUris =
                        {
                            new Uri("https://localhost:44378/callback/login/local")
                        },
                        PostLogoutRedirectUris =
                        {
                            new Uri("https://localhost:44378/callback/logout/local")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.Endpoints.Token,
                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.ResponseTypes.Code,
                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    });
                }
            }).GetAwaiter().GetResult();
        }
    }
}
