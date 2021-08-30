using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Matty.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("+-------------------------------------------------------------------+");
            Console.WriteLine("             Welcome to Matty a device code flow sample");
            Console.WriteLine("+-------------------------------------------------------------------+");
            Console.Write(" - Please press Enter after the server started...");
            Console.ReadLine();

            using var client = new HttpClient();

            try
            {
                // Make call to "/.well-known/openid-configuration" endpoint
                // to discover identity server configuration document
                var discovery = await client.GetDiscoveryDocumentAsync("https://localhost:44321");
                if (discovery.IsError)
                {
                    throw new Exception(discovery.Error);
                }

                var deviceResponse = await client.RequestDeviceAuthorizationAsync(new DeviceAuthorizationRequest
                { 
                    Address = discovery.DeviceAuthorizationEndpoint,
                    Scope = "openid offline_access profile email",
                    ClientId = "device",
                });
                if (deviceResponse.IsError)
                {
                    throw new Exception(deviceResponse.Error);
                }

                Console.WriteLine("+-------------------------------------------------------------------+");
                Console.WriteLine("Device endpoint response");
                Console.WriteLine($" - Device code: {deviceResponse.DeviceCode}");
                Console.WriteLine($" - User code: {deviceResponse.UserCode}");
                Console.WriteLine($" - Verification uri: {deviceResponse.VerificationUri}");
                Console.WriteLine($" - Verification uri (complete): {deviceResponse.VerificationUriComplete}");

                Console.WriteLine("+-------------------------------------------------------------------+");
                Console.WriteLine(" - Please open the verification uri in the browser");
                Console.WriteLine(" and enter the user code to authorize this device ");
                Console.WriteLine("+-------------------------------------------------------------------+");
                Console.Write(" - Please press Enter to proceed...");
                Console.ReadLine();
                TokenResponse tokenResponse;
                do
                {
                    tokenResponse = await client.RequestDeviceTokenAsync(new DeviceTokenRequest
                    {
                        Address = discovery.TokenEndpoint,
                        ClientId = "device",
                        DeviceCode = deviceResponse.DeviceCode
                    });

                    if (tokenResponse is { IsError: true, Error: Errors.AuthorizationPending })
                    {
                        Console.WriteLine(" - authorization pending...");

                        // Note: `deviceResponse.Interval` is the minimum number of seconds
                        // the client should wait between polling requests.
                        // In this sample the client will retry every 60 seconds at least.
                        await Task.Delay(Math.Max(deviceResponse.Interval, 60) * 1000);
                    }
                    else if (tokenResponse.IsError)
                    {
                        throw new Exception(tokenResponse.Error);
                    }
                    else
                    {
                        Console.WriteLine("+-------------------------------------------------------------------+");
                        Console.WriteLine("Token endpoint response");
                        Console.WriteLine($" - Identity token: {tokenResponse.IdentityToken}");
                        Console.WriteLine();
                        Console.WriteLine($" - Access token: {tokenResponse.AccessToken}");
                        Console.WriteLine();
                        Console.WriteLine($" - Refresh token: {tokenResponse.RefreshToken}");
                        Console.WriteLine();
                        break;
                    }
                    
                } while (true);

                Console.WriteLine("+-------------------------------------------------------------------+");
                var resource = await GetResourceAsync(client, tokenResponse.AccessToken);
                Console.WriteLine($" - API response: {resource}");
                Console.ReadLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine("+-------------------------------------------------------------------+");
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.InnerException?.Message);
                Console.WriteLine(" - Make sure you started the authorization server.");
                Console.WriteLine("+-------------------------------------------------------------------+");
                Console.ReadLine();
            }
        }

        public static async Task<string> GetResourceAsync(HttpClient client, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44321/api/message");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
