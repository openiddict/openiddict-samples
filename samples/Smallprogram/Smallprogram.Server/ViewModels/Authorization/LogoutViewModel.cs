using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Smallprogram.Server.ViewModels.Authorization
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}
