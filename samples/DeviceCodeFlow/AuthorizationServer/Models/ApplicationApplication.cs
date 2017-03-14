using OpenIddict.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationServer.Models
{
    public class ApplicationApplication : 
        OpenIddictApplication<string, 
            ApplicationAuthorization, 
            OpenIddictToken<string, ApplicationApplication, ApplicationAuthorization>>
    {
        public ApplicationApplication()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
