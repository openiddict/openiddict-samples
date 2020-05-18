using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Velusia.Server.ViewModels.Authorization
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}
