using Microsoft.EntityFrameworkCore;

namespace Aridka.Server.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder) { }
    }
}
