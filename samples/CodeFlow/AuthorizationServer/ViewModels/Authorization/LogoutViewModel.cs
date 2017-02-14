using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AuthorizationServer.ViewModels.Authorization
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}
