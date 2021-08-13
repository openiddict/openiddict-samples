using System.Text.Json.Serialization;

namespace Gonda.Client.Models
{
    public class TokensResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
        
        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }
        
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}