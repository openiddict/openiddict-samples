using System.Linq;
using AuthorizationServer.Models;
using AuthorizationServer.Services;
using CryptoHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Core;
using OpenIddict.Models;

namespace AuthorizationServer {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<ApplicationDbContext>(options => {
                // Configure the context to use an in-memory store.
                options.UseInMemoryDatabase();

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Register the OpenIddict services.
            services.AddOpenIddict()
                // Register the Entity Framework stores.
                .AddEntityFrameworkCoreStores<ApplicationDbContext>()

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                .AddMvcBinders()

                // Enable the authorization, logout, userinfo, and introspection endpoints.
                .EnableAuthorizationEndpoint("/connect/authorize")
                .EnableLogoutEndpoint("/connect/logout")
                .EnableIntrospectionEndpoint("/connect/introspect")
                .EnableUserinfoEndpoint("/Account/Userinfo")

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

        public void Configure(IApplicationBuilder app) {
            app.UseIdentity();

            app.UseCors(builder => {
                builder.WithOrigins("http://localhost:9000");
                builder.WithMethods("GET");
                builder.WithHeaders("Authorization");
            });

            app.UseOpenIddict();

            app.UseOAuthValidation();

            app.UseMvcWithDefaultRoute();

            // In a production app, seed this in a setup tool.
            SeedDatabase(app);
        }

        private void SeedDatabase(IApplicationBuilder app) {
            var options = app
                .ApplicationServices
                .GetRequiredService<DbContextOptions<ApplicationDbContext>>();

            using (var context = new ApplicationDbContext(options)) {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var applications = context.Set<OpenIddictApplication>();

                if (!applications.Any()) {
                    applications.Add(new OpenIddictApplication {
                        ClientId = "Aurelia.OpenIdConnect",
                        DisplayName = "Aurelia Open Id Connect",
                        LogoutRedirectUri = "http://localhost:9000/signout-oidc",
                        RedirectUri = "http://localhost:9000/signin-oidc",
                        Type = OpenIddictConstants.ClientTypes.Public
                    });

                    applications.Add(new OpenIddictApplication {
                        ClientId = "ResourceServer01",
                        ClientSecret = Crypto.HashPassword("secret_secret_secret"),
                        Type = OpenIddictConstants.ClientTypes.Confidential
                    });

                    applications.Add(new OpenIddictApplication {
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