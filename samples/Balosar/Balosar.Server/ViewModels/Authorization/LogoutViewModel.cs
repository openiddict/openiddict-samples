using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Balosar.Server.ViewModels.Authorization
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}