using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IdentityModel.Client;

using var client = new HttpClient();

const string email = "bob@le-magnifique.com", password = "}s>EWG@f4g;_v7nB";

await CreateAccountAsync(client, email, password);

var token = await GetTokenAsync(client, email, password);
Console.WriteLine("Access token: {0}", token);
Console.WriteLine();

var resource = await GetResourceAsync(client, token);
Console.WriteLine("API response: {0}", resource);

Console.ReadLine();

static async Task CreateAccountAsync(HttpClient client, string email, string password)
{
    var response = await client.PostAsJsonAsync("https://localhost:44360/Account/Register", new { email, password });

    // Ignore 409 responses, as they indicate that the account already exists.
    if (response.StatusCode == HttpStatusCode.Conflict)
    {
        return;
    }

    response.EnsureSuccessStatusCode();
}

static async Task<string> GetTokenAsync(HttpClient client, string email, string password)
{
    // Retrieve the OpenIddict server configuration document containing the endpoint URLs.
    var configuration = await client.GetDiscoveryDocumentAsync("https://localhost:44360/");
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
    using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44360/api/message");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}
