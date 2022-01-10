using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Quartz;
using Smallprogram.Server.Data;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Smallprogram.Server
{
    public class Worker : IJob
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<Worker> logger;

        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = serviceProvider.CreateScope();

            var dbcontext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // use ef-core cli migration 'dotnet ef database update'

            await dbcontext.Database.EnsureDeletedAsync();
            await dbcontext.Database.EnsureCreatedAsync();

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            #region Register Client

            if (await manager.FindByClientIdAsync("blazor-Server-client") is null)
            {
                logger.LogInformation("Register Blazor Server Client");
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "blazor-Server-client",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "BlazorServer client Test application",
                    ClientSecret = "7f71f2f5-2fd6-43bb-9b5a-19c5b52d602d",
                    Type = ClientTypes.Confidential,
                    PostLogoutRedirectUris =
                    {
                        new Uri("http://localhost:8011/signout-callback-oidc"),
                        new Uri("https://localhost:8010/signout-callback-oidc"),
                    },
                    RedirectUris =
                    {
                        new Uri("http://localhost:8011/signin-oidc"),
                        new Uri("https://localhost:8010/signin-oidc")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "api1",
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            if (await manager.FindByClientIdAsync("blazor-wasm-client") is null)
            {
                logger.LogInformation("Register Blazor WebAssembly Client");
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "blazor-wasm-client",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "BlazorWASM client application",
                    Type = ClientTypes.Public,
                    PostLogoutRedirectUris =
                    {
                        new Uri("http://localhost:8001/authentication/logout-callback"),
                        new Uri("https://localhost:8000/authentication/logout-callback")
                    },
                    RedirectUris =
                    {
                        new Uri("http://localhost:8001/authentication/login-callback"),
                        new Uri("https://localhost:8000/authentication/login-callback")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "api1",
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            #endregion

            #region Register Scopes
            var scopesmanager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByClientIdAsync("resource_server_1") == null)
            {
                logger.LogInformation("init api1");
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = "resource_server_1",
                    ClientSecret = "1de4db00-e45b-4200-8d9a-0a6317d8e307",
                    Permissions =
                        {
                            Permissions.Endpoints.Introspection
                        }
                };

                await manager.CreateAsync(descriptor);
            }

            if (await scopesmanager.FindByNameAsync("api1") is null)
            {
                await scopesmanager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    DisplayName = "demo api1 access",
                    Name = "api1",
                    Resources =
                    {
                        "resource_server_1"
                    }
                });
            }


            #endregion

            #region Register User And Role
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!dbcontext.Users.Any())
            {
                var role = new IdentityRole("admin");
                await roleManager.CreateAsync(role);

                var user = new IdentityUser("zhusir");
                user.Email = "smallprogram@foxmail.com";
                await userManager.CreateAsync(user, "123456");
                await userManager.AddToRoleAsync(user, "admin");
            }

            #endregion
        }
    }
}
