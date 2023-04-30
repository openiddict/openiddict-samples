using System.Data.Entity;

namespace Mortis.Client.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.UseOpenIddict();

            base.OnModelCreating(modelBuilder);
        }
    }
}
