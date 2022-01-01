using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;

Console.WriteLine("+-------------------------------------------------------------------+");
Console.WriteLine("             Welcome to Matty a device code flow sample");
Console.WriteLine("+-------------------------------------------------------------------+");
Console.Write(" - Please press Enter after the server started...");
Console.ReadLine();

using var client = new HttpClient();

try
{
    // Retrieve the OpenIddict server configuration document containing the endpoint URLs.
    var configuration = await client.GetDiscoveryDocumentAsync("https://localhost:44321/");
    if (configuration.IsError)
    {
        throw new Exception($"An error occurred while retrieving the configuration document: {configuration.Error}");
    }

    var deviceResponse = await client.RequestDeviceAuthorizationAsync(new DeviceAuthorizationRequest
    { 
        Address = configuration.DeviceAuthorizationEndpoint,
        Scope = "openid offline_access profile email",
        ClientId = "device"
    });
    if (deviceResponse.IsError)
    {
        throw new Exception($"An error occurred while retrieving a device code: {deviceResponse.Error}");
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
                
    TokenResponse tokenResponse;
    do
    {
        tokenResponse = await client.RequestDeviceTokenAsync(new DeviceTokenRequest
        {
            Address = configuration.TokenEndpoint,
            ClientId = "device",
            DeviceCode = deviceResponse.DeviceCode
        });

        if (tokenResponse is { IsError: true, Error: OidcConstants.TokenErrors.AuthorizationPending })
        {
            Console.WriteLine(" - authorization pending...");

            // Note: `deviceResponse.Interval` is the minimum number of seconds
            // the client should wait between polling requests.
            // In this sample the client will retry every 60 seconds at most.
            await Task.Delay(Math.Clamp(deviceResponse.Interval, 1, 60) * 1000);
        }

        else if (tokenResponse.IsError)
        {
            throw new Exception($"An error occurred while retrieving an access token: {tokenResponse.Error}");
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
    }
    while (true);

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

static async Task<string> GetResourceAsync(HttpClient client, string token)
{
    using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44321/api/message");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}
