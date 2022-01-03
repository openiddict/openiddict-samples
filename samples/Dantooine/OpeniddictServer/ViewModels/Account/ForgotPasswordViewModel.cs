using System.ComponentModel.DataAnnotations;

namespace OpeniddictServer.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
