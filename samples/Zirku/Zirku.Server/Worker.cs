using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using Zirku.Server.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Zirku.Server
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            await CreateApplicationsAsync();
            await CreateScopesAsync();

            async Task CreateApplicationsAsync()
            {
                var manager = scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication>>();

                if (await manager.FindByClientIdAsync("aurelia") == null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "aurelia",
                        DisplayName = "Aurelia client application",
                        PostLogoutRedirectUris =
                        {
                            new Uri("http://localhost:9000/signout-oidc")
                        },
                        RedirectUris =
                        {
                            new Uri("http://localhost:9000/signin-oidc")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.GrantTypes.Implicit,
                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "api1",
                            Permissions.Prefixes.Scope + "api2"
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
                            Permissions.Endpoints.Introspection
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
                            Permissions.Endpoints.Introspection
                        }
                    };

                    await manager.CreateAsync(descriptor);
                }
            }

            async Task CreateScopesAsync()
            {
                var manager = scope.ServiceProvider.GetRequiredService<OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope>>();

                if (await manager.FindByNameAsync("api1") == null)
                {
                    var descriptor = new OpenIddictScopeDescriptor
                    {
                        Name = "api1",
                        Resources =
                        {
                            "resource_server_1"
                        }
                    };

                    await manager.CreateAsync(descriptor);
                }

                if (await manager.FindByNameAsync("api2") == null)
                {
                    var descriptor = new OpenIddictScopeDescriptor
                    {
                        Name = "api2",
                        Resources =
                        {
                            "resource_server_2"
                        }
                    };

                    await manager.CreateAsync(descriptor);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
