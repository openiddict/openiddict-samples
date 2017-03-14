using AspNet.Security.OpenIdConnect.Primitives;
using AuthorizationServer.Models;
using AuthorizationServer.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.DeviceCodeFlow;
using OpenIddict.DeviceCodeFlow.EntityFrameworkCore;
using OpenIddict.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;

namespace AuthorizationServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            services.AddSingleton<IConfiguration>(configBuilder.Build());

            services.AddMvc();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use an in-memory store.
                //options.UseInMemoryDatabase();
                var connection = @"Server=(localdb)\mssqllocaldb;Database=AuthorizationServer;Trusted_Connection=True;";
                options.UseSqlServer(connection);

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            // Register the Identity services.
            services.AddIdentity<ApplicationUser, IdentityRole<string>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            services.AddSingleton(new DeviceCodeOptions());
            
            services.AddScoped<IDeviceCodeStore<ApplicationDeviceCode, string>,
                DeviceCodeStore<ApplicationDeviceCode,ApplicationDbContext>>();
            
            services.AddScoped<DeviceCodeManager<ApplicationDeviceCode>>();

            // Register the OpenIddict services.
            services.AddOpenIddict<ApplicationApplication, 
                ApplicationAuthorization, 
                OpenIddictScope<string>, 
                OpenIddictToken<string, ApplicationApplication, ApplicationAuthorization>>(options =>
            {
                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<ApplicationDbContext>();

                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.AddMvcBinders();

                // Enable the token endpoint.
                options.EnableTokenEndpoint("/connect/token");

                // Enable the password flow.
                options.AllowDeviceCodeFlow();

                // During development, you can disable the HTTPS requirement.
                options.DisableHttpsRequirement();

                // Note: to use JWT access tokens instead of the default
                // encrypted format, the following lines are required:
                //
                // options.UseJsonWebTokens();
                // options.AddEphemeralSigningKey();
            });
            
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            loggerFactory.AddConsole(configuration.GetSection("Logging"));

            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), branch =>
            {
                // Add a middleware used to validate access
                // tokens and protect the API endpoints.
                branch.UseOAuthValidation();

                // If you prefer using JWT, don't forget to disable the automatic
                // JWT -> WS-Federation claims mapping used by the JWT middleware:
                //
                // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
                // JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
                //
                // app.UseJwtBearerAuthentication(new JwtBearerOptions
                // {
                //     Authority = "http://localhost:58795/",
                //     Audience = "resource_server",
                //     RequireHttpsMetadata = false,
                //     TokenValidationParameters = new TokenValidationParameters
                //     {
                //         NameClaimType = OpenIdConnectConstants.Claims.Subject,
                //         RoleClaimType = OpenIdConnectConstants.Claims.Role
                //     }
                // });

                // Alternatively, you can also use the introspection middleware.
                // Using it is recommended if your resource server is in a
                // different application/separated from the authorization server.
                //
                // app.UseOAuthIntrospection(options =>
                // {
                //     options.Authority = new Uri("http://localhost:58795/");
                //     options.Audiences.Add("resource_server");
                //     options.ClientId = "resource_server";
                //     options.ClientSecret = "875sqd4s5d748z78z7ds1ff8zz8814ff88ed8ea4z4zzd";
                //     options.RequireHttpsMetadata = false;
                // });
            });

            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), branch =>
            {
                // Insert a new cookies middleware in the pipeline to store the user
                // identity after he has been redirected from the identity provider.
                branch.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true,
                    LoginPath = new PathString("/signin")
                });

                branch.UseIdentity();
            });

            app.UseOpenIddict();

            app.UseMvcWithDefaultRoute();

            InitializeAsync(app.ApplicationServices).GetAwaiter().GetResult();
        }

        private async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
                using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await context.Database.EnsureCreatedAsync(cancellationToken);

                    var manager =
                        scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager<ApplicationApplication>>();

                    if (await manager.FindByClientIdAsync("console", cancellationToken) == null)
                    {
                        var application = new ApplicationApplication
                        {
                            ClientId = "console",
                            DisplayName = "My client application"
                        };

                        await
                            manager.CreateAsync(application, "388D45FA-B36B-4988-BA59-B187D329C207", cancellationToken);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"error initializing app: {e}");
                throw e;
            }
        }
    }
}
