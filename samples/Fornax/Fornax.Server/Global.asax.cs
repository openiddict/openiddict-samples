using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Web;
using Fornax.Server.Models;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Fornax.Server
{
    public class Global : HttpApplication, IContainerProviderAccessor
    {
        public static IContainerProvider Provider { get; private set; }

        IContainerProvider IContainerProviderAccessor.ContainerProvider => Provider;

        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

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
                    // Enable the authorization and token endpoints.
                    options.SetAuthorizationEndpointUris("/connect/authorize")
                           .SetTokenEndpointUris("/connect/token");

                    // Mark the "email", "profile" and "roles" scopes as supported scopes.
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                    // Note: this sample only uses the authorization code flow but you can enable
                    // the other flows if you need to support implicit, password or client credentials.
                    options.AllowAuthorizationCodeFlow();

                    // Register the signing and encryption credentials.
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    // Register the OWIN host and configure the OWIN-specific options.
                    //
                    // Note: unlike other samples, this sample doesn't use token endpoint pass-through
                    // to handle token requests in a custom MVC action. As such, the token requests
                    // will be automatically handled by OpenIddict, that will reuse the identity
                    // resolved from the authorization code to produce access and identity tokens.
                    //
                    options.UseOwin()
                           .EnableAuthorizationEndpointPassthrough();
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

            Provider = new ContainerProvider(builder.Build());

            // Seed the database with the sample client using the OpenIddict application manager.
            // Note: in a real world application, this step should be part of a setup script.
            Task.Run(async delegate
            {
                using var scope = Provider.ApplicationContainer.BeginLifetimeScope();

                var context = scope.Resolve<ApplicationDbContext>();
                context.Database.CreateIfNotExists();

                var manager = scope.Resolve<IOpenIddictApplicationManager>();

                if (await manager.FindByClientIdAsync("console_app") == null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "console_app",
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "Console application",
                        RedirectUris =
                        {
                            new Uri("http://localhost:7891/")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Token,
                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.ResponseTypes.Code
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