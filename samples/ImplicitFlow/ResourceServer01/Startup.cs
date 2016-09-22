using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceServer01
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var authServerHost = "http://localhost:12345";

            app.UseOAuthIntrospection(options =>
            {
                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;
                options.Authority = authServerHost;
                options.Audiences.Add("ResourceServer01");
                options.ClientId = "ResourceServer01";
                options.ClientSecret = "secret_secret_secret";
            });

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:9000");
                builder.WithMethods("GET");
                builder.WithHeaders("Authorization");
            });

            app.UseMvc((routes) =>
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
