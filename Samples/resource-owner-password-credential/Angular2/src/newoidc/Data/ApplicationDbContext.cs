using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using newoidc.Models;
using OpenIddict;
using System.Security.Claims;
using AspNet.Security.OpenIdConnect.Extensions;

namespace newoidc.Data
{
    public class ApplicationDbContext : OpenIddictContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
      
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductPicture> ProductPicture { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<offer> offer { get; set; }
        public DbSet<OfferDetail> OfferDetail { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<kartErrors> kartErrors { get; set; }
        public DbSet<tempCart> tempCarts { get; set; }
    }

   
}
