using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;

using var client = new HttpClient();

var token = await GetTokenAsync(client, "alice@wonderland.com", "P@ssw0rd");
Console.WriteLine("Access token: {0}", token);
Console.WriteLine();

var resource = await GetResourceAsync(client, token);
Console.WriteLine("API response: {0}", resource);

Console.ReadLine();

static async Task<string> GetTokenAsync(HttpClient client, string email, string password)
{
    // Retrieve the OpenIddict server configuration document containing the endpoint URLs.
    var configuration = await client.GetDiscoveryDocumentAsync("http://localhost:58779/");
    if (configuration.IsError)
    {
        throw new Exception($"An error occurred while retrieving the configuration document: {configuration.Error}");
    }

    var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
    {
        Address = configuration.TokenEndpoint,
        UserName = email,
        Password = password
    });

    if (response.IsError)
    {
        throw new Exception($"An error occurred while retrieving an access token: {response.Error}");
    }

    return response.AccessToken;
}

static async Task<string> GetResourceAsync(HttpClient client, string token)
{
    using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:58779/api/message");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}
