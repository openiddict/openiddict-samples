using System;
using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Zirku.Api2
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = OAuthIntrospectionDefaults.AuthenticationScheme;
            })

            .AddOAuthIntrospection(options =>
            {
                options.Authority = new Uri("http://localhost:12345/");
                options.Audiences.Add("resource_server_2");
                options.ClientId = "resource_server_2";
                options.ClientSecret = "C744604A-CD05-4092-9CF8-ECB7DC3499A2";
                options.RequireHttpsMetadata = false;

                // Note: you can override the default name and role claims:
                // options.NameClaimType = "custom_name_claim";
                // options.RoleClaimType = "custom_role_claim";
            });

            // If you prefer using JWT, don't forget to disable the automatic
            // JWT -> WS-Federation claims mapping used by the JWT middleware:
            //
            // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            // JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            //
            // services.AddAuthentication(options =>
            // {
            //     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            // })
            //
            // .AddJwtBearer(options =>
            // {
            //     options.Authority = "http://localhost:12345/";
            //     options.Audience = "resource_server_2";
            //     options.RequireHttpsMetadata = false;
            //     options.TokenValidationParameters = new TokenValidationParameters
            //     {
            //         NameClaimType = OpenIdConnectConstants.Claims.Subject,
            //         RoleClaimType = OpenIdConnectConstants.Claims.Role
            //     };
            // });

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:9000");
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
