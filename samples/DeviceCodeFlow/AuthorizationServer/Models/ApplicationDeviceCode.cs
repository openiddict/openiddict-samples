using OpenIddict.DeviceCodeFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenIddict.DeviceCodeFlow.EntityFrameworkCore;

namespace AuthorizationServer.Models
{
    public class ApplicationDeviceCode : OpenIddictDeviceCode<string>
    {
        public ApplicationDeviceCode()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
