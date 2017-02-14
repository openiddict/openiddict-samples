using System.ComponentModel.DataAnnotations;

namespace AuthorizationServer.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
