using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NgOidc.Data;
using NgOidc.Models;
using OpenIddict;
using CryptoHelper;
using NWebsec.AspNetCore.Middleware;
using System;

namespace NgOidc
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc();
            // Register Identity services.
            services.AddIdentity<ApplicationUser, IdentityRole>
                        (o =>
                                {
                                    o.Password.RequireDigit = false;
                                    o.Password.RequireLowercase = false;
                                    o.Password.RequireUppercase = false;
                                    o.Password.RequireNonAlphanumeric = false;
                                    o.Password.RequiredLength = 6;
                                    o.Cookies.ApplicationCookie.AutomaticChallenge = false;
                                }
                        ).AddEntityFrameworkStores<ApplicationDbContext>()
                         .AddDefaultTokenProviders();


            services.AddOpenIddict<ApplicationUser, IdentityRole, ApplicationDbContext>()
                      .UseJsonWebTokens()
                      .EnableAuthorizationEndpoint("/connect/authorize")
                      .EnableTokenEndpoint("/connect/token")
                      .EnableUserinfoEndpoint("/connect/userinfo")
                      .EnableRevocationEndpoint("/connect/revoke")
                      //Other flows not needed for this example.
                      .AllowRefreshTokenFlow()
                      .AllowPasswordFlow()
                      // During development, you can disable the HTTPS requirement.
                      .DisableHttpsRequirement();

            //IMPORTANT !!! Change CORS policy on production server
            services.AddCors(
                options =>
                      {
                          options.AddPolicy("AllowAllOrigin",
                          builder => builder.AllowAnyOrigin()
                                            .AllowAnyMethod()
                                            .AllowCredentials()
                                            .AllowAnyHeader()
                                           );
                      });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("AllowAllOrigin");

            app.UseStaticFiles();

            loggerFactory.AddDebug();

            app.UseCsp(options => options.DefaultSources(directive => directive.Self().CustomSources("*"))
                           .ImageSources(directive => directive.Self()
                                .CustomSources("*", "data:"))

                           .ScriptSources(directive => directive.Self()
                                .UnsafeEval()
                                .UnsafeInline()
                                .CustomSources("*"))

                           .StyleSources(directive => directive.Self()
                             .CustomSources("*")
                                .UnsafeInline())
                      );

            app.UseXContentTypeOptions();

            app.UseXfo(options => options.Deny());

            app.UseXXssProtection(options => options.EnabledWithBlockMode());

            app.UseIdentity(); // Use identitry before openiddict

            //Add JWT middleware
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = false,
                Audience = "http://localhost:3000",
                Authority = "http://localhost:52606"
            });

            app.UseOpenIddict();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

            });

            /// Add Client application to your database
            using (var context = new ApplicationDbContext(
                      app.ApplicationServices.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                context.Database.EnsureCreated();

                if (!context.Applications.Any())
                {
                    context.Applications.Add(new OpenIddictApplication
                    {
                        ClientId = "localApp",
                        DisplayName = "Angular 2 client application",
                        RedirectUri = "http://localhost:3000/signin-oidc",
                        LogoutRedirectUri = "http://localhost:3000/",
                        ClientSecret = Crypto.HashPassword("secret_secret_secret"),
                        Type = OpenIddictConstants.ClientTypes.Public // note that its a public Client for confidential you will need to send different parameters from client.
                    });

                    context.SaveChanges();
                }
            }
        }
    }
}

