using AuthorizationServer.BindingModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AuthorizationServer
{
    public interface IExternalAuthorizationManager
    {
        Task<ExternalProfileBindingModel> GetProfile(string accessToken, ExternalAuthProviders provider);
        Task<bool> VerifyExternalAccessToken(string accessToken, ExternalAuthProviders provider);
    }

    public enum ExternalAuthProviders
    {
        Facebook,
        Google
    }

    public class ExternalAuthorizationManager : IExternalAuthorizationManager
    {
        private readonly IConfiguration _configuration;

        public ExternalAuthorizationManager(
            IConfiguration configuration
            )
        {
            _configuration = configuration;
        }

        public async Task<ExternalProfileBindingModel> GetProfile(string accessToken, ExternalAuthProviders provider)
        {
            if(provider == ExternalAuthProviders.Facebook)
            {
                var url = $"https://graph.facebook.com/me?fields=email,first_name,last_name&access_token={ accessToken }";
                var client = new HttpClient();
                var uri = new Uri(url);
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var profile = JsonConvert.DeserializeObject<ExternalProfileBindingModel>(content);
                    return profile;
                }
                return null;
            }

            if (provider == ExternalAuthProviders.Google)
            {
                var url = $"https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token={ accessToken }";
                var client = new HttpClient();
                var uri = new Uri(url);
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var profile = JsonConvert.DeserializeObject<ExternalProfileBindingModel>(content);

                    //just ot make it easier for the method that calls this object
                    profile.first_name = profile.given_name;
                    profile.last_name = profile.last_name;
                    return profile;
                }
                return null;
            }

            return null;


        }

        public async Task<bool> VerifyExternalAccessToken(string accessToken, ExternalAuthProviders provider)
        {
            var verifyEndpoint = string.Empty;

            if (provider == ExternalAuthProviders.Facebook)
            {
                var appToken = _configuration["Authentication:External:Facebook:appToken"];
                verifyEndpoint = $"https://graph.facebook.com/debug_token?input_token={ accessToken }&access_token={ appToken }";
            }

            if (provider == ExternalAuthProviders.Google)
            {
                verifyEndpoint = $"https://www.googleapis.com/oauth2/v3/tokeninfo?access_token={ accessToken }";
            }

            if (string.IsNullOrWhiteSpace(verifyEndpoint))
            {
                return false;
            }

            var client = new HttpClient();
            var uri = new Uri(verifyEndpoint);
            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();             

                if (provider == ExternalAuthProviders.Facebook)
                {
                    var result = JsonConvert.DeserializeObject<FacebookDebugTokenBindingModel>(content);

                    if (!_configuration["Authentication:External:Facebook:appId"].Equals(result.Data.App_Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                }
                else if (provider == ExternalAuthProviders.Google)
                {
                    var result = JsonConvert.DeserializeObject<GoogleTokenInfoBindingModel>(content);
                    
                    if (!_configuration["Authentication:External:Google:clientId"].Equals(result.azp, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            }

            //request failed so something probably wrong with the token
            return false;
        }
    }
}
