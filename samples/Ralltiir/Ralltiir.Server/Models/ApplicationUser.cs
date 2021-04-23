using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Ralltiir.Server.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Key]
        public Guid UserId { get; set; }
        
        // public IEnumerable<Guid> Workspaces { get; set; }
    }
}