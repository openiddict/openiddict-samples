using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;

namespace AuthorizationServer.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            ExternalAccounts = new HashSet<ExternalAccount>();
        }
        
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<ExternalAccount> ExternalAccounts { get; set; }
    }
}
