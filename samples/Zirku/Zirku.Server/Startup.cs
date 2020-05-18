using System;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Zirku.Server.Models;
using Zirku.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace Zirku.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            services.AddOpenIddict()

                // Register the OpenIddict core services.
                .AddCore(options =>
                {
                    // Register the Entity Framework stores and models.
                    options.UseEntityFrameworkCore()
                           .UseDbContext<ApplicationDbContext>();
                })

                // Register the OpenIddict server handler.
                .AddServer(options =>
                {
                    // Enable the authorization, logout, userinfo, and introspection endpoints.
                    options.EnableAuthorizationEndpoint("/connect/authorize")
                           .EnableLogoutEndpoint("/connect/logout")
                           .EnableIntrospectionEndpoint("/connect/introspect")
                           .EnableUserinfoEndpoint("/api/userinfo");

                    // Mark the "email", "profile" and "roles" scopes as supported scopes.
                    options.RegisterScopes(OpenIdConnectConstants.Scopes.Email,
                                           OpenIdConnectConstants.Scopes.Profile,
                                           OpenIddictConstants.Scopes.Roles);

                    // Note: the sample only uses the implicit code flow but you can enable
                    // the other flows if you need to support implicit, password or client credentials.
                    options.AllowImplicitFlow();

                    // During development, you can disable the HTTPS requirement.
                    options.DisableHttpsRequirement();

                    // Register a new ephemeral key, that is discarded when the application
                    // shuts down. Tokens signed using this key are automatically invalidated.
                    // This method should only be used during development.
                    options.AddEphemeralSigningKey();

                    // On production, using a X.509 certificate stored in the machine store is recommended.
                    // You can generate a self-signed certificate using Pluralsight's self-cert utility:
                    // https://s3.amazonaws.com/pluralsight-free/keith-brown/samples/SelfCert.zip
                    //
                    // options.AddSigningCertificate("7D2A741FE34CC2C7369237A5F2078988E17A6A75");
                    //
                    // Alternatively, you can also store the certificate as an embedded .pfx resource
                    // directly in this assembly or in a file published alongside this project:
                    //
                    // options.AddSigningCertificate(
                    //     assembly: typeof(Startup).GetTypeInfo().Assembly,
                    //     resource: "Zirku.Server.Certificate.pfx",
                    //     password: "OpenIddict");

                    // Note: to use JWT access tokens instead of the default
                    // encrypted format, the following line is required:
                    //
                    // options.UseJsonWebTokens();
                })

                // Register the OpenIddict validation handler.
                // Note: the OpenIddict validation handler is only compatible with the
                // default token format or with reference tokens and cannot be used with
                // JWT tokens. For JWT tokens, use the Microsoft JWT bearer handler.
                .AddValidation();

            services.AddCors();
            services.AddControllersWithViews();

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
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
                options.MapControllers();
                options.MapDefaultControllerRoute();
            });

            // Seed the database with the sample applications.
            // Note: in a real world application, this step should be part of a setup script.
            InitializeAsync(app.ApplicationServices).GetAwaiter().GetResult();
        }

        private async Task InitializeAsync(IServiceProvider services)
        {
            // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureCreatedAsync();

                await CreateApplicationsAsync();
                await CreateScopesAsync();

                async Task CreateApplicationsAsync()
                {
                    var manager = scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictApplication>>();

                    if (await manager.FindByClientIdAsync("aurelia") == null)
                    {
                        var descriptor = new OpenIddictApplicationDescriptor
                        {
                            ClientId = "aurelia",
                            DisplayName = "Aurelia client application",
                            PostLogoutRedirectUris = { new Uri("http://localhost:9000/signout-oidc") },
                            RedirectUris = { new Uri("http://localhost:9000/signin-oidc") },
                            Permissions =
                            {
                                OpenIddictConstants.Permissions.Endpoints.Authorization,
                                OpenIddictConstants.Permissions.Endpoints.Logout,
                                OpenIddictConstants.Permissions.GrantTypes.Implicit,
                                OpenIddictConstants.Permissions.Scopes.Email,
                                OpenIddictConstants.Permissions.Scopes.Profile,
                                OpenIddictConstants.Permissions.Scopes.Roles,
                                OpenIddictConstants.Permissions.Prefixes.Scope + "api1",
                                OpenIddictConstants.Permissions.Prefixes.Scope + "api2"
                            }
                        };

                        await manager.CreateAsync(descriptor);
                    }

                    if (await manager.FindByClientIdAsync("resource_server_1") == null)
                    {
                        var descriptor = new OpenIddictApplicationDescriptor
                        {
                            ClientId = "resource_server_1",
                            ClientSecret = "846B62D0-DEF9-4215-A99D-86E6B8DAB342",
                            Permissions =
                            {
                                OpenIddictConstants.Permissions.Endpoints.Introspection
                            }
                        };

                        await manager.CreateAsync(descriptor);
                    }

                    if (await manager.FindByClientIdAsync("resource_server_2") == null)
                    {
                        var descriptor = new OpenIddictApplicationDescriptor
                        {
                            ClientId = "resource_server_2",
                            ClientSecret = "C744604A-CD05-4092-9CF8-ECB7DC3499A2",
                            Permissions =
                            {
                                OpenIddictConstants.Permissions.Endpoints.Introspection
                            }
                        };

                        await manager.CreateAsync(descriptor);
                    }
                }

                async Task CreateScopesAsync()
                {
                    var manager = scope.ServiceProvider.GetRequiredService<OpenIddictScopeManager<OpenIddictScope>>();

                    if (await manager.FindByNameAsync("api1") == null)
                    {
                        var descriptor = new OpenIddictScopeDescriptor
                        {
                            Name = "api1",
                            Resources = { "resource_server_1" }
                        };

                        await manager.CreateAsync(descriptor);
                    }

                    if (await manager.FindByNameAsync("api2") == null)
                    {
                        var descriptor = new OpenIddictScopeDescriptor
                        {
                            Name = "api2",
                            Resources = { "resource_server_2" }
                        };

                        await manager.CreateAsync(descriptor);
                    }
                }
            }
        }
    }
}