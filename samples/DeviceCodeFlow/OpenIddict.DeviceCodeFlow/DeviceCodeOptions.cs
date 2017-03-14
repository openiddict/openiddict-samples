using System;

namespace OpenIddict.DeviceCodeFlow
{
    public class DeviceCodeOptions
    {
        public int UserCodeLength { get; set; } = 12;
        public int DeviceCodeLength { get; set; } = 26;
        public int Interval { get; set; } = 4;
        public int DeviceCodeDuration { get; set; } = 300;
    }
}