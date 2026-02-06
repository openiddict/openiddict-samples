
using Geonosis.Auth.Data;

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
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
