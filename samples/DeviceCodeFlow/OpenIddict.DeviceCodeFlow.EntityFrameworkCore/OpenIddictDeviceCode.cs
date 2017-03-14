using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OpenIddict.Models;

namespace OpenIddict.DeviceCodeFlow.EntityFrameworkCore
{
    public class OpenIddictDeviceCode : OpenIddictDeviceCode<string>
    {
        public OpenIddictDeviceCode()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class OpenIddictDeviceCode<TKey>
        where TKey: IEquatable<TKey>
    {
        public virtual TKey Id { get; set; }

        public virtual DateTimeOffset CreatedOn { get; set; }

        public virtual DateTimeOffset ExpiresAt { get; set; }

        public virtual TKey Application { get; set; }

        public virtual string DeviceCode { get; set; }

        public virtual string UserCode { get; set; }

        public virtual DateTimeOffset AuthorizedOn { get; set; }

        public virtual TKey Subject { get; set; }

        public virtual string Scope { get; set; }

        public virtual IEnumerable<string> GetScopes()
        {
            return from s in Scope.Split(' ') where s.Length != 0 select s;
        }
    }
}
