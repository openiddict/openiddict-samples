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
            using var scope = _serviceProvider.CreateScope();

            // Create the database.
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            await SeedClientsAsync(scope.ServiceProvider, cancellationToken);
            await SeedRolesAsync(scope.ServiceProvider, cancellationToken);
            await SeedSampleUsersAsync(scope.ServiceProvider, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private static async Task SeedSampleUsersAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            // Create an admin user if it doesn't exist.
            // NOTE: In a production application, you would likely want to have a more robust seeding strategy and not hard-code credentials.
            var defaultUser = await userManager.FindByNameAsync("admin");
            if (defaultUser == null)
            {
                defaultUser = new ApplicationUser
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

                defaultUser.PasswordHash = userManager.PasswordHasher.HashPassword(defaultUser, "Admin@12345");

                await userManager.CreateAsync(defaultUser);
            }
        }

        private static async Task SeedRolesAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
        }

        private static async Task SeedClientsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

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

                    Permissions.ResponseTypes.Code,

                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange,
                }
            };

            if (uiClient == null)
            {
                await applicationManager.CreateAsync(uiClientApplicationDescriptor, cancellationToken);
            }
            else
            {
                await applicationManager.UpdateAsync(uiClient, uiClientApplicationDescriptor, cancellationToken);
            }
        }
    }
}
