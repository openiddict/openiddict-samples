using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenIddict.Models;

namespace AuthorizationServer.Models
{
    public class ApplicationAuthorization : 
        OpenIddictAuthorization<string, 
            ApplicationApplication, 
            OpenIddictToken<string, ApplicationApplication, ApplicationAuthorization>>
    {
        public ApplicationAuthorization()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
