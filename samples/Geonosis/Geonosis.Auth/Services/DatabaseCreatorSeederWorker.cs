using System.Globalization;
using Geonosis.Auth.Data;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Geonosis.Auth.Services
{
    internal sealed class DatabaseCreatorSeederWorker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseCreatorSeederWorker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            // Create the database.
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            await SeedClientsAsync(_serviceProvider, cancellationToken);
            await SeedRolesAsync(_serviceProvider, cancellationToken);
            await SeedSampleUsersAsync(_serviceProvider, cancellationToken);
            await SeedScopesAsync(_serviceProvider, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private static async Task SeedSampleUsersAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateAsyncScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            // Create an admin user if it doesn't exist.
            // NOTE: In a production application, you would likely want to have a more robust seeding strategy and not hard-code credentials.
            var defaultAdminUser = await userManager.FindByNameAsync("admin");
            if (defaultAdminUser == null)
            {
                defaultAdminUser = new ApplicationUser
                {
                    UserName = "admin@contoso.com",
                    NormalizedUserName = "ADMIN@CONTOSO.COM",
                    Email = "admin@contoso.COM",
                    NormalizedEmail = "ADMIN@CONTOSO.COM",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    TwoFactorEnabled = false,
                    AccessFailedCount = 0,
                };

                await userManager.CreateAsync(defaultAdminUser, "Admin@12345");
                await userManager.AddToRoleAsync(defaultAdminUser, "Admin");
            }

            var defaultUser = await userManager.FindByNameAsync("user");
            if (defaultUser == null)
            {
                defaultUser = new ApplicationUser
                {
                    UserName = "user@contoso.com",
                    NormalizedUserName = "USER@CONTOSO.COM",
                    Email = "user@contoso.COM",
                    NormalizedEmail = "USER@CONTOSO.COM",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    TwoFactorEnabled = false,
                    AccessFailedCount = 0,
                };

                await userManager.CreateAsync(defaultUser, "User@12345");
            }
        }

        private static async Task SeedRolesAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateAsyncScope();

            // Create an admin role if it doesn't exist.
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
            if (!adminRoleExists)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
        }

        private static async Task SeedClientsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateAsyncScope();

            var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            // Create the client application representing the UI if it doesn't exist.
            var uiClient = await applicationManager.FindByClientIdAsync("geonosis-ui", cancellationToken);
            var uiClientApplicationDescriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = "geonosis-ui",
                ClientSecret = "super-secret-client-secret",
                DisplayName = "Monarch UI Application",
                // Web application using the BFF authentication model is considered a confidential client because the server-side BFF
                // component can securely store the client secret and it handle all the interactions with the authorization server on behalf
                // of the client application, including token management and refreshing.
                ClientType = ClientTypes.Confidential,
                // Implicit consent type for public clients, no need to prompt the user for consent in this sample,
                // but in a production application, you should consider the appropriate consent type based on your application's requirements
                // and user experience goals.
                ConsentType = ConsentTypes.Implicit,
                // RedirectUris must match the URLs used by the Blazor Web application during the authentication process
                // These URLs are where the authorization server will redirect the user after login/logout back to the client application
                RedirectUris =
                {
                    new Uri("http://localhost:5027/authentication/login-callback/local"),
                    new Uri("https://localhost:7073/authentication/login-callback/local"),
                },
                PostLogoutRedirectUris =
                {
                    new Uri("http://localhost:5027/authentication/logout-callback/local"),
                    new Uri("https://localhost:7073/authentication/logout-callback/local"),
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.EndSession,
                    Permissions.Endpoints.Token,

                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    // The token exchange grant type is required for the UI to exchange the access token it receives
                    // from the authorization server for a new access token that can be used to call the API.
                    Permissions.GrantTypes.TokenExchange,

                    Permissions.ResponseTypes.Code,

                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,

                    // Custom scope representing the API scope that the UI will request access to using the token exchange flow.
                    Permissions.Prefixes.Scope + "Weather.Read",
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange,
                },
            };

            if (uiClient == null)
            {
                await applicationManager.CreateAsync(uiClientApplicationDescriptor, cancellationToken);
            }
            else
            {
                await applicationManager.UpdateAsync(uiClient, uiClientApplicationDescriptor, cancellationToken);
            }

            // Create the client application representing the API if it doesn't exist.
            var apiClient = await applicationManager.FindByClientIdAsync("geonosis-api", cancellationToken);
            var apiClientApplicationDescriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = "geonosis-api",
                ClientSecret = "super-secret-client-secret-2",
                DisplayName = "Geonosis API Application",
                ClientType = ClientTypes.Confidential,
                ConsentType = ConsentTypes.Implicit,
            };

            if (apiClient == null)
            {
                await applicationManager.CreateAsync(apiClientApplicationDescriptor, cancellationToken);
            }
            else
            {
                await applicationManager.UpdateAsync(apiClient, apiClientApplicationDescriptor, cancellationToken);
            }
        }

        public static async Task SeedScopesAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateAsyncScope();

            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            var weatherReadApiScope = await scopeManager.FindByNameAsync("Weather.Read", cancellationToken);
            var weatherReadApiScopeDescriptor = new OpenIddictScopeDescriptor
            {
                Name = "Weather.Read",
                DisplayName = "Weather Read API Scope",
                DisplayNames =
                {
                    [CultureInfo.GetCultureInfo("en-US")] = "Weather Read API Scope"
                },
                Description = "Scope for reading weather data from the API",
                Resources =
                {
                    "geonosis-api"
                }
            };

            if (weatherReadApiScope == null)
            {
                await scopeManager.CreateAsync(weatherReadApiScopeDescriptor, cancellationToken);
            }
            else
            {
                await scopeManager.UpdateAsync(weatherReadApiScope, weatherReadApiScopeDescriptor, cancellationToken);
            }
        }
    }
}