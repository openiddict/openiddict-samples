using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace OpenIddict.DeviceCodeFlow
{
    public static class OpenIddictBuilderExtensions
    {
        public static OpenIddictBuilder AllowDeviceCodeFlow(this OpenIddictBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Configure(options =>
                options.GrantTypes.Add(DeviceCodeFlowConstants.GrantTypes.DeviceCode)
            );
        }
    }
}
