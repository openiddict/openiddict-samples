using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Zirku.Server.ViewModels.Authorization
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}
