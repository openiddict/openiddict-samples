using AspNet.Security.OpenIdConnect.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationServer.ViewModels.Authorization
{
    public class AuthorizeDeviceCodeViewModel
    {
        public string UserCode { get; set; }
        public string Scope { get; set; }
        public string ApplicationName { get; set; }
    }
}
