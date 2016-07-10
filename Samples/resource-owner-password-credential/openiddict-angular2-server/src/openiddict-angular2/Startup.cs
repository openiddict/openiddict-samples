using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using openiddict_angular2.Data;
using openiddict_angular2.Models;
using openiddict_angular2.Services;
using OpenIddict;
using CryptoHelper;
using System.IO;
using NWebsec.AspNetCore.Middleware;
namespace openiddict_angular2
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
                // Register the Identity services.
                services.AddIdentity<ApplicationUser, IdentityRole>(o =>
                {
                    o.Password.RequireDigit = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequiredLength = 6;
                    o.Cookies.ApplicationCookie.AutomaticChallenge = false;

                })
                 .AddEntityFrameworkStores<ApplicationDbContext>()
                 //.AddTokenProvider()
                 .AddDefaultTokenProviders();
                services.AddOpenIddict<ApplicationUser, IdentityRole, ApplicationDbContext>()
                       .UseJsonWebTokens()
                    .SetAuthorizationEndpointPath("/connect/authorize")
                    .SetLogoutEndpointPath("/connect/logout")

                    // During development, you can disable the HTTPS requirement.
                    .DisableHttpsRequirement();

            //IMPORTANT !!! Change CORS policy on production server
                services.AddCors(options =>
                {
                    options.AddPolicy("AllowSpecificOrigin",
                        builder => builder.AllowAnyOrigin()
                                         .AllowAnyMethod()
                                         .AllowCredentials()
                                         .WithExposedHeaders("")
                                        .AllowAnyHeader());
                });

                // Add application services.
                services.AddTransient<IEmailSender, AuthMessageSender>();
                services.AddTransient<ISmsSender, AuthMessageSender>();

            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
                app.UseCors("AllowSpecificOrigin");
                app.UseStaticFiles();
                loggerFactory.AddDebug();

            //CSP Customisation
                app.UseCsp(options => options.DefaultSources(directive => directive.Self().CustomSources("*"))
                   .ImageSources(directive => directive.Self()
                        .CustomSources("*", "data:"))
                   .ScriptSources(directive => directive.Self()
                        .UnsafeEval()
                        .UnsafeInline()
                        .CustomSources("*"))
                   .StyleSources(directive => directive.Self()
                     .CustomSources("*")
                        .UnsafeInline()));

                app.UseXContentTypeOptions();

                app.UseXfo(options => options.Deny());

                app.UseXXssProtection(options => options.EnabledWithBlockMode());
             
                app.UseIdentity();

                app.UseGoogleAuthentication(new GoogleOptions
                {
                    ClientId = "862227465575-q1spfclcfvflg4tpesfkle4e0jc3q987.apps.googleusercontent.com",
                    ClientSecret = "ozzg2VvH2TYbSbBYWE_HIYG5"

                });
               

                app.UseJwtBearerAuthentication(new JwtBearerOptions
                {
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true,
                    RequireHttpsMetadata = false,
                    Audience = "http://localhost:52606",
                    Authority = "http://localhost:52606"
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
                            ClientId = "localApp",
                            DisplayName = "MVC Core client application",
                            RedirectUri = "http://localhost:52992/signin-oidc",
                            LogoutRedirectUri = "http://localhost:52992/",
                            ClientSecret = Crypto.HashPassword("secret_secret_secret"),
                            Type = OpenIddictConstants.ClientTypes.Public
                        });

                       

                        context.SaveChanges();
                    }
                }
            }
        }
    }

