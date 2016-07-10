using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict;
using newoidc.Data;
using newoidc.Models;
using newoidc.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NWebsec.AspNetCore.Middleware;
using System.Linq;
using CryptoHelper;

namespace newoidc
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            }).AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();

            services.AddOpenIddict<ApplicationUser, ApplicationDbContext>()
                .UseJsonWebTokens()
                .AddMvc()
                .AddAssets()
                .AddNWebsec(options =>
                options.DefaultSources(directive => directive.Self()
                    .CustomSources("*"))

                .ImageSources(directive => directive.Self()
                    .CustomSources("*", "data:"))

                .ScriptSources(directive => directive.Self()
                    .UnsafeEval()
                    .UnsafeInline()
                    .CustomSources("*"))

                .StyleSources(directive => directive.Self()
                    .CustomSources("*")
                    .UnsafeInline()))
            // During development, you can disable the HTTPS requirement.
                .DisableHttpsRequirement();
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddConsole();
            app.UseStaticFiles();
            app.UseIdentity();
            app.UseGoogleAuthentication(new GoogleOptions
            {
                ClientId = "862227465575-q1spfclcfvflg4tpesfkle4e0jc3q987.apps.googleusercontent.com",
                ClientSecret = "ozzg2VvH2TYbSbBYWE_HIYG5"

            });

            app.UseTwitterAuthentication(new TwitterOptions
            {
                ConsumerKey = "6XaCTaLbMqfj6ww3zvZ5g",
                ConsumerSecret = "Il2eFzGIrYhz6BWjYhVXBPQSfZuS4xoHpSSyD9PI"
            });
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = false,
                Audience = "http://localhost:58056/",
                Authority = "http://localhost:58056/"
            });
            app.UseOpenIddict();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "spa-fallback",
                    template: "{*url}",
                    defaults: new { controller = "Home", action = "Index" });
            });

            /// below stuff is for development purpose to generate database
            using (var context = new ApplicationDbContext(
    app.ApplicationServices.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                context.Database.EnsureCreated();

                if (!context.Applications.Any())
                {
                    context.Applications.Add(new OpenIddictApplication
                    {
                        // Assign a unique identifier to your client app:
                        Id = "myClient",

                        // Assign a display named used in the consent form page:
                        DisplayName = "MVC Core client application",

                        // Register the appropriate redirect_uri and post_logout_redirect_uri:
                        RedirectUri = "http://localhost:58056/signin-oidc",
                        LogoutRedirectUri = "http://localhost:58056/",

                        // Generate a new derived key from the client secret:
                        Secret = Crypto.HashPassword("secret_secret_secret"),

                        // Note: use "public" for JS/mobile/desktop applications
                        // and "confidential" for server-side applications.
                        Type = OpenIddictConstants.ClientTypes.Public
                    });

                    context.SaveChanges();
                }
            }
        }
    }
}
