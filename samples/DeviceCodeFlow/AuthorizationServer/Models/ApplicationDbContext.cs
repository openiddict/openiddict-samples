using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.DeviceCodeFlow;
using OpenIddict.Models;

namespace AuthorizationServer.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<string>, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>,IdentityRoleClaim<string>,IdentityUserToken<string>>
    {
        public DbSet<ApplicationDeviceCode> DeviceCodes { get; set; }
        public DbSet<ApplicationAuthorization> Authorizations { get; set; }
        public DbSet<ApplicationApplication> Applications { get; set; }
        public DbSet<OpenIddictToken> Tokens { get; set; }

        public ApplicationDbContext(DbContextOptions options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
