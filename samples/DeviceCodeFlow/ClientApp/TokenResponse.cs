using System;

namespace ClientApp
{
    public class TokenResponse
    {
        public string Resource { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
    }
}