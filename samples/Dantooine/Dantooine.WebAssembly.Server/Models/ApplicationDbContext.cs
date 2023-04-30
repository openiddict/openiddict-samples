using Microsoft.EntityFrameworkCore;

namespace Dantooine.WebAssembly.Server.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }
}
