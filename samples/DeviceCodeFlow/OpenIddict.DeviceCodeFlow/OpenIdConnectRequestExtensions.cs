using System;
using System.Collections.Generic;
using System.Text;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.Extensions.Primitives;

namespace OpenIddict.DeviceCodeFlow
{
    public static class OpenIdConnectRequestExtensions
    {
        public static bool IsDeviceCodeGrantType(this OpenIdConnectRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.GrantType))
            {
                return false;
            }

            var segment = TrimUtils.Trim(new StringSegment(request.GrantType), OpenIdConnectConstants.Separators.Space);
            if (segment.Length == 0)
            {
                return false;
            }

            return segment.Equals(DeviceCodeFlowConstants.GrantTypes.DeviceCode, StringComparison.Ordinal);
        }

        public static bool IsDeviceCodeFlow(this OpenIdConnectRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.ResponseType))
            {
                return false;
            }

            var segment = TrimUtils.Trim(new StringSegment(request.ResponseType), OpenIdConnectConstants.Separators.Space);
            if (segment.Length == 0)
            {
                return false;
            }

            return segment.Equals(DeviceCodeFlowConstants.ResponseTypes.DeviceCode, StringComparison.Ordinal);
        }
    }
}
