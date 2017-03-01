using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceServer02
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseOAuthIntrospection(options =>
            {
                options.Authority = new Uri("http://localhost:12345");
                options.Audiences.Add("resource-server-2");
                options.ClientId = "resource-server-2";
                options.ClientSecret = "C744604A-CD05-4092-9CF8-ECB7DC3499A2";
                options.RequireHttpsMetadata = false;

                // Note: you can override the default name and role claims:
                // options.NameClaimType = "custom_name_claim";
                // options.RoleClaimType = "custom_role_claim";
            });

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:9000");
                builder.WithMethods("GET");
                builder.WithHeaders("Authorization");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "empty",
                    template: "{controller=Resource}/{action=Public}"
                );

                routes.MapRoute(
                    name: "api",
                    template: "api/{action=Public}",
                    defaults: new { controller = "Resource" }
                );
            });
        }
    }
}
