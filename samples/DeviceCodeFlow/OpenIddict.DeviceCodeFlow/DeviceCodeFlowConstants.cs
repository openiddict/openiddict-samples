using System;
using System.Collections.Generic;
using System.Text;

namespace OpenIddict.DeviceCodeFlow
{
    public static class DeviceCodeFlowConstants
    {
        public static class ResponseTypes
        {
            public const string DeviceCode = "device_code";
        }

        public static class GrantTypes
        {
            public const string DeviceCode = "urn:ietf:params:oauth:grant-type:device_code";
        }

        public static class Errors
        {
            public const string DeviceCodeAuthorizationPending = "authorization_pending";
            public const string SlowDown = "slow_down";
        }
    }
}
