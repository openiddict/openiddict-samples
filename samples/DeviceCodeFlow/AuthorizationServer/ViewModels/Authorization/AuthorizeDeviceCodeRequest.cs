using AspNet.Security.OpenIdConnect.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationServer.ViewModels.Authorization
{
    public class AuthorizeDeviceCodeRequest : OpenIdConnectRequest
    {
        public string UserCode { get; set; }
    }
}
