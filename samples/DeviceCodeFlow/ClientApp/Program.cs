using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace ClientApp
{
    public class Program
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy(true, false)
            }
        };

        public static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        public static async Task MainAsync(string[] args)
        {
            var client = new HttpClient();

            // setup
            const string clientId = "console", clientSecret = "388D45FA-B36B-4988-BA59-B187D329C207";
            const string scope = "openid";

            // 1. user initiates client device login

            var code = await GetDeviceCodeAsync(client, clientId, clientSecret, scope);

            // 2. device prompts user to authorize code
            // either by printing the url to the console or something like this:

            Console.WriteLine($"Verification uri: {code.VerificationUri}");
            Console.WriteLine($"User code: {code.UserCode}");
            OpenBrowser(code.VerificationUri);

            // 3. user authorizes code in the browser 
            // while the device client polls for success
            
            var token = await GetTokenAsync(client, code.Interval, clientId, clientSecret, code.DeviceCode);

            
            Console.WriteLine("Access token: {0}", token);
            Console.WriteLine();

            var resource = await GetResourceAsync(client, token.AccessToken);
            Console.WriteLine("API response: {0}", resource);

            Console.ReadLine();
        }

        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start(new ProcessStartInfo("xdg-open", url));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start(new ProcessStartInfo("open", url));
            }
            else
            {
                Console.WriteLine($"please navigate to {url} to authorize this device");
            }
        }

        public static async Task<DeviceCodeResponse> GetDeviceCodeAsync(HttpClient client, string clientId,
            string clientSecret, string scope)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:58795/connect/device_token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = scope,
                ["response_type"] = "device_code"
            });

            Console.WriteLine("requesting device token");

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine("response:\n" + body);

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<DeviceCodeResponse>(body, jsonSettings);
        }

        public static async Task<TokenResponse> GetTokenAsync(HttpClient client, int interval, string clientId, string clientSecret, string deviceCode)
        {
            while (true)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:58795/connect/token");
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["device_code"] = deviceCode,
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code"
                });

                var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine("response: \n" + content);

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<TokenResponse>(content, jsonSettings);
                }

                var jsonResponse = JsonConvert.DeserializeObject<ErrorResponse>(content, jsonSettings);

                if (jsonResponse == null)
                {
                    throw new Exception("null json response object from content");
                }
                if (jsonResponse.Error.Equals("slow_down"))
                {
                    interval++;
                }

                Console.WriteLine($"waiting {interval} seconds");
                
                await Task.Delay(interval*1000);
            }
        }

        public static async Task CreateAccountAsync(HttpClient client, string email, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:58795/Account/Register");
            request.Content = new StringContent(JsonConvert.SerializeObject(new { email, password }), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();
        }

        public static async Task<string> GetResourceAsync(HttpClient client, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:58795/api/message");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
