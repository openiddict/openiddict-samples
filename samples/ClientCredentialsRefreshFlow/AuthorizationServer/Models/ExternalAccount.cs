using System;
using System.ComponentModel.DataAnnotations;

namespace AuthorizationServer.Models
{
    public class ExternalAccount
    {
        public string Id { get; set; }

        [Required]
        public ExternalAuthProviders Provider { get; set; }

        [Required]
        public string ProviderUserId { get; set; }

        public DateTimeOffset AddedAt { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
