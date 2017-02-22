using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceServer01
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
                options.Audiences.Add("resource-server-1");
                options.ClientId = "resource-server-1";
                options.ClientSecret = "846B62D0-DEF9-4215-A99D-86E6B8DAB342";
                options.RequireHttpsMetadata = false;
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
