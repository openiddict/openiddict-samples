using Newtonsoft.Json;

namespace Gonda.Client.Models
{
    public class TokensResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
        
        [JsonProperty(PropertyName = "id_token")]
        public string IdToken { get; set; }
        
        [JsonProperty(PropertyName = "expires_in")]
        public string ExpiresIn { get; set; }
        
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }
    }
}