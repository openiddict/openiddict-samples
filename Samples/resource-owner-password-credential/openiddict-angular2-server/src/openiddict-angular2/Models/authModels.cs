using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace openiddict_angular2.Models
{
    public class AuthorizeViewModel
    {
        [Display(Name = "Application")]
        public string ApplicationName { get; set; }

        [BindNever]
        public IDictionary<string, string> Parameters { get; set; }

        [Display(Name = "Scope")]
        public string Scope { get; set; }
    }

    public class LogoutViewModel
    {
        [BindNever]
        public IDictionary<string, string> Parameters { get; set; }
    }

    public class ErrorViewModel
    {
        [Display(Name = "Error")]
        public string Error { get; set; }

        [Display(Name = "Description")]
        public string ErrorDescription { get; set; }
    }

}
