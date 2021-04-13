using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using Gonda.Server.EF;
using Gonda.Server.Models;

namespace Gonda.Server
{
    public class DataMigration : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DataMigration(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            await CreateApplicationsAsync();
            await CreateScopesAsync();

            async Task CreateApplicationsAsync()
            {
                var manager = scope.ServiceProvider
                    .GetRequiredService<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>>>();

                if (await manager.FindByClientIdAsync("gonda-client") == null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "gonda-client",
                        DisplayName = "Gonda.Client",
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.GrantTypes.Password,
                            OpenIddictConstants.Permissions.ResponseTypes.IdToken,
                            OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken,
                            OpenIddictConstants.Permissions.ResponseTypes.Token,
                            OpenIddictConstants.Permissions.Scopes.Email,
                            OpenIddictConstants.Permissions.Scopes.Profile,
                            OpenIddictConstants.Permissions.Scopes.Roles,
                            OpenIddictConstants.Permissions.Prefixes.Scope + "api1"
                        }
                    };

                    await manager.CreateAsync(descriptor);
                }
                
                if (await manager.FindByClientIdAsync("gonda-api1", cancellationToken) == null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        DisplayName = "Gonda.Api1",
                        ClientId = "gonda-api1",
                        ClientSecret = "63956dcd-8ff8-45c4-8005-d05ea82a3861",
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Introspection
                        }
                    }, cancellationToken);
                }
            }

            async Task CreateScopesAsync()
            {
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

                if (await manager.FindByNameAsync("api1") == null)
                {
                    var descriptor = new OpenIddictScopeDescriptor
                    {
                        Name = "api1",
                        Resources =
                        {
                            "gonda-api1"
                        }
                    };

                    await manager.CreateAsync(descriptor);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}