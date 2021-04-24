using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Gonda.Server.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Key]
        public Guid UserId { get; set; }
        
        // public IEnumerable<Guid> Workspaces { get; set; }
    }
}