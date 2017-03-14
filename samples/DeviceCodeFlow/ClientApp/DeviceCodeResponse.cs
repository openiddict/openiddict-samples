using System;

namespace ClientApp
{
    public class DeviceCodeResponse
    {
        public string UserCode { get; set; }
        public string DeviceCode { get; set; }
        public string VerificationUri { get; set; }
        public int ExpiresIn { get; set; }
        public int Interval { get; set; }
    }
}