using AuthorizationServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthorizationServer
{
    public class Startup
    {

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                //Authentication:External:Facebook:appToken
                //Authentication:External:Facebook:appId
                //Authentication:External:Google:clientId
                builder.AddUserSecrets("OppenIddict.Samples.ClientCredentialsRerfreshFlow.AuthorizationServer");
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var env = services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>();
            
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddTransient< IExternalAuthorizationManager, ExternalAuthorizationManager>();

            services.AddMvc();

            services.AddDbContext<ApplicationDbContext>(options => {
                options.UseInMemoryDatabase();
            });
            
           
            // Register the Identity services.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            var builder = services.AddOpenIddict<ApplicationDbContext>()
               .AddMvcBinders()
               .EnableTokenEndpoint("/connect/token")
               .AllowPasswordFlow()
               .AllowRefreshTokenFlow()                                   //or should this just be external then check in the controller
               .AllowCustomFlow("urn:ietf:params:oauth:grant-type:external_identity_token")            

                // During development, you can disable the HTTPS requirement.
                .DisableHttpsRequirement()

                // Register a new ephemeral key, that is discarded when the application
                // shuts down. Tokens signed using this key are automatically invalidated.
                // This method should only be used during development.
                .AddEphemeralSigningKey();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentity();

            app.UseCors(builder => {
                builder.WithOrigins("http://localhost:9000");
                builder.WithMethods("GET");
                builder.WithHeaders("Authorization");
            });

            app.UseOpenIddict();

            app.UseOAuthValidation();

            app.UseMvcWithDefaultRoute();
        }
    }
}
