using AuthorizationServer.Models;
using AuthorizationServer.Services;
using CryptoHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict;
using System;
using System.Linq;

namespace AuthorizationServer
{
    public class Startup
    {
        private IConfiguration _configuration;
        public void ConfigureServices(IServiceCollection services)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite("Filename=./authorization.db");
            });

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext, Guid>();

            // Register the OpenIddict services, including the default Entity Framework stores.
            services.AddOpenIddict<ApplicationUser, IdentityRole<Guid>, ApplicationDbContext, Guid>()

                // Enable the authorization, logout, userinfo, and introspection endpoints.
                .EnableAuthorizationEndpoint("/connect/authorize")
                .EnableLogoutEndpoint("/connect/logout")
                .EnableUserinfoEndpoint("/connect/userinfo")
                .EnableIntrospectionEndpoint("/connect/introspect")

                // Note: the sample only uses the implicit code flow but you can enable
                // the other flows if you need to support implicit, password or client credentials.
                .AllowImplicitFlow()

                // During development, you can disable the HTTPS requirement.
                .DisableHttpsRequirement()

                // Register a new ephemeral key, that is discarded when the application
                // shuts down. Tokens signed using this key are automatically invalidated.
                // This method should only be used during development.
                .AddEphemeralSigningKey();

            services.AddCors();
            services.AddMvc();

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentity();

            app.UseCors(builder =>
            {
                builder.WithOrigins(
                    _configuration["aureliaHost"], 
                    _configuration["oidcClientHost"]
                );
                builder.WithMethods("GET");
            });

            app.UseOpenIddict();
            app.UseMvcWithDefaultRoute();

            // In a production app, seed this in a setup tool.
            SeedDatabase(app);
        }

        private void SeedDatabase(IApplicationBuilder app)
        {
            var options = app
                .ApplicationServices
                .GetRequiredService<DbContextOptions<ApplicationDbContext>>();

            using (var context = new ApplicationDbContext(options))
            {
                // drop and recreate
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                if (!context.Applications.Any())
                {
                    var aureliaHost = _configuration["aureliaHost"];

                    context.Applications.Add(new OpenIddictApplication<Guid>
                    {
                        ClientId = "Aurelia.OpenIdConnect",
                        DisplayName = "Aurelia Open Id Connect",
                        LogoutRedirectUri = $"{aureliaHost}/signout-oidc",
                        RedirectUri = $"{aureliaHost}/signin-oidc",
                        Type = OpenIddictConstants.ClientTypes.Public
                    });

                    var oidcClientHost = _configuration["oidcClientHost"];
                    
                    context.Applications.Add(new OpenIddictApplication<Guid>
                    {
                        ClientId = "OidcClientJs.OidcClient",
                        DisplayName = "Oidc Client Js - Oidc Client Sample",
                        LogoutRedirectUri = $"{oidcClientHost}/oidc-client-sample.html",
                        RedirectUri = $"{oidcClientHost}/oidc-client-sample.html",
                        Type = OpenIddictConstants.ClientTypes.Public
                    });

                    context.Applications.Add(new OpenIddictApplication<Guid>
                    {
                        ClientId = "OidcClientJs.UserManager",
                        DisplayName = "Oidc Client Js - User Manager Sample",
                        LogoutRedirectUri = $"{oidcClientHost}/user-manager-sample.html",
                        RedirectUri = $"{oidcClientHost}/user-manager-sample.html",
                        Type = OpenIddictConstants.ClientTypes.Public
                    });

                    context.Applications.Add(new OpenIddictApplication<Guid>
                    {
                        ClientId = "ResourceServer01",
                        ClientSecret = Crypto.HashPassword("secret_secret_secret"),
                        Type = OpenIddictConstants.ClientTypes.Confidential
                    });

                    context.Applications.Add(new OpenIddictApplication<Guid>
                    {
                        ClientId = "ResourceServer02",
                        ClientSecret = Crypto.HashPassword("secret_secret_secret"),
                        Type = OpenIddictConstants.ClientTypes.Confidential
                    });
                }

                context.SaveChanges();
            }
        }
    }
}