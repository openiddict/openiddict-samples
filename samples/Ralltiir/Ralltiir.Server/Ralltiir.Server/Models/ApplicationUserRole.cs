using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Ralltiir.Server.Models
{
    public class ApplicationUserRole : IdentityRole<Guid>
    {
        [Key]
        public Guid RoleId { get; set; }
    }
}