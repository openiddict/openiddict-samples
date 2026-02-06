
using Geonosis.Auth.Data;
using Microsoft.AspNetCore.Identity;

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
    }
}
