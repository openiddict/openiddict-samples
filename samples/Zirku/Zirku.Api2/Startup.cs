using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;

namespace Zirku.Api2
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            // Register the OpenIddict validation components.
            services.AddOpenIddict()
                .AddValidation(options =>
                {
                    // Note: the validation handler uses OpenID Connect discovery
                    // to retrieve the issuer signing keys used to validate tokens.
                    options.SetIssuer("https://localhost:44319/");
                    options.AddAudiences("resource_server_2");

                    // Register the encryption credentials. This sample uses a symmetric
                    // encryption key that is shared between the server and the Api2 sample
                    // (that performs local token validation instead of using introspection).
                    //
                    // Note: in a real world application, this encryption key should be
                    // stored in a safe place (e.g in Azure KeyVault, stored as a secret).
                    options.AddEncryptionKey(new SymmetricSecurityKey(
                        Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

                    // Register the System.Net.Http integration.
                    options.UseSystemNetHttp();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseCors(builder =>
            {
                builder.WithOrigins("https://localhost:44398");
                builder.WithMethods("GET");
                builder.WithHeaders("Authorization");
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(options =>
            {
                options.MapControllerRoute(
                    name: "empty",
                    pattern: "{controller=Resource}/{action=Public}"
                );

                options.MapControllerRoute(
                    name: "api",
                    pattern: "api/{action=Public}",
                    defaults: new { controller = "Resource" }
                );
            });
        }
    }
}
