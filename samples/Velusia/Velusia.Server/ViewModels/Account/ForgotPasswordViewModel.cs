using System.ComponentModel.DataAnnotations;

namespace Velusia.Server.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
