using System.ComponentModel.DataAnnotations;

namespace Smallprogram.Server.ViewModels.Authorization
{
    public class AuthorizeViewModel
    {
        [Display(Name = "Application Name")]
        public string ApplicationName { get; set; }

        [Display(Name = "Scope")]
        public string Scope { get; set; }
    }
}
