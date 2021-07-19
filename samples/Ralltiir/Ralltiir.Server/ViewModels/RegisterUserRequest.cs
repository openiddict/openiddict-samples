using System.ComponentModel.DataAnnotations;

namespace Ralltiir.Server.ViewModels
{
    public class RegisterUserRequest
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        [Required]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match")]
        public string ConfirmPassword { get; set; }
    }
}