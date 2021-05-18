using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using OpenIddict.EntityFrameworkCore.Models;

namespace Rentor.Server.Data
{
    public class RentorToken : OpenIddictEntityFrameworkCoreToken<int, RentorApplication, RentorAuthorization>
    {
    }
}
