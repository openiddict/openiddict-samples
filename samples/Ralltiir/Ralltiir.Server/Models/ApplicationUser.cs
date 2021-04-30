using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Ralltiir.Server.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
    }
}