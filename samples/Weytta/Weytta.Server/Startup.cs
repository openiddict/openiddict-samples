using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Weytta.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<DbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseInMemoryDatabase("db");

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            // Register the Negotiate handler (when running on IIS, it will automatically
            // delegate the actual Integrated Windows Authentication process to IIS).
            services.AddAuthentication()
                .AddNegotiate();

            services.AddOpenIddict()

                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                    options.UseEntityFrameworkCore()
                           .UseDbContext<DbContext>();
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

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    //
                    // Note: unlike other samples, this sample doesn't use token endpoint pass-through
                    // to handle token requests in a custom MVC action. As such, the token requests
                    // will be automatically handled by OpenIddict, that will reuse the identity
                    // resolved from the authorization code to produce access and identity tokens.
                    //
                    options.UseAspNetCore()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableStatusCodePagesIntegration();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });

            // Register the worker responsible of seeding the database with the sample clients.
            // Note: in a real world application, this step should be part of a setup script.
            services.AddHostedService<Worker>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(options =>
            {
                options.MapControllers();
                options.MapDefaultControllerRoute();
            });

            app.UseWelcomePage();
        }
    }
}
