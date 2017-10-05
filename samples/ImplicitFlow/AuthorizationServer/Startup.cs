using System;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AuthorizationServer.Extensions;
using AuthorizationServer.Models;
using AuthorizationServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Core;
using OpenIddict.Models;

namespace AuthorizationServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .AddEnvironmentVariables()
                .Build();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlServer(configuration["Data:DefaultConnection:ConnectionString"]);

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            // Register the OpenIddict services.
            services.AddOpenIddict(options =>
            {
                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<ApplicationDbContext>();

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.AddMvcBinders();

                // Enable the authorization, logout, userinfo, and introspection endpoints.
                options.EnableAuthorizationEndpoint("/connect/authorize")
                       .EnableLogoutEndpoint("/connect/logout")
                       .EnableIntrospectionEndpoint("/connect/introspect")
                       .EnableUserinfoEndpoint("/api/userinfo");

                // Note: the sample only uses the implicit code flow but you can enable
                // the other flows if you need to support implicit, password or client credentials.
                options.AllowImplicitFlow();

                // During development, you can disable the HTTPS requirement.
                options.DisableHttpsRequirement();

                // Register a new ephemeral key, that is discarded when the application
                // shuts down. Tokens signed using this key are automatically invalidated.
                // This method should only be used during development.
                options.AddEphemeralSigningKey();

                // On production, using a X.509 certificate stored in the machine store is recommended.
                // You can generate a self-signed certificate using Pluralsight's self-cert utility:
                // https://s3.amazonaws.com/pluralsight-free/keith-brown/samples/SelfCert.zip
                //
                // options.AddSigningCertificate("7D2A741FE34CC2C7369237A5F2078988E17A6A75");
                //
                // Alternatively, you can also store the certificate as an embedded .pfx resource
                // directly in this assembly or in a file published alongside this project:
                //
                // options.AddSigningCertificate(
                //     assembly: typeof(Startup).GetTypeInfo().Assembly,
                //     resource: "AuthorizationServer.Certificate.pfx",
                //     password: "OpenIddict");

                // Note: to use JWT access tokens instead of the default
                // encrypted format, the following line is required:
                //
                // options.UseJsonWebTokens();
            });

            services.AddCors();
            services.AddMvc();

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:9000");
                builder.WithMethods("GET");
                builder.WithHeaders("Authorization");
            });

            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), branch =>
            {
                branch.UseIdentity();
            });

            app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), branch =>
            {
                branch.UseOAuthValidation();
            });

            app.UseOpenIddict();

            app.UseMvcWithDefaultRoute();

            // Seed the database with the sample applications.
            // Note: in a real world application, this step should be part of a setup script.
            InitializeAsync(app.ApplicationServices, CancellationToken.None).GetAwaiter().GetResult();
        }

        private async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureCreatedAsync();

                var manager = scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictApplication>>();

                if (await manager.FindByClientIdAsync("aurelia", cancellationToken) == null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "aurelia",
                        DisplayName = "Aurelia client application",
                        PostLogoutRedirectUris = { new Uri("http://localhost:9000/signout-oidc") },
                        RedirectUris = { new Uri("http://localhost:9000/signin-oidc") }
                    };

                    await manager.CreateAsync(descriptor, cancellationToken);
                }

                if (await manager.FindByClientIdAsync("resource-server-1", cancellationToken) == null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "resource-server-1",
                        ClientSecret = "846B62D0-DEF9-4215-A99D-86E6B8DAB342"
                    };

                    await manager.CreateAsync(descriptor, cancellationToken);
                }

                if (await manager.FindByClientIdAsync("resource-server-2", cancellationToken) == null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "resource-server-2",
                        ClientSecret = "C744604A-CD05-4092-9CF8-ECB7DC3499A2"
                    };

                    await manager.CreateAsync(descriptor, cancellationToken);
                }
            }
        }
    }
}