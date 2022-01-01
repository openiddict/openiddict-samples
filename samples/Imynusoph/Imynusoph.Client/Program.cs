using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;

using var client = new HttpClient();

const string email = "bob@le-magnifique.com", password = "}s>EWG@f4g;_v7nB";

await CreateAccountAsync(client, email, password);

var tokens = await GetTokensAsync(client, email, password);
Console.WriteLine("Initial access token: {0}", tokens.AccessToken);
Console.WriteLine();
Console.WriteLine("Initial refresh token: {0}", tokens.RefreshToken);

Console.WriteLine();
Console.WriteLine();

tokens = await RefreshTokensAsync(client, tokens.RefreshToken);
Console.WriteLine("New access token: {0}", tokens.AccessToken);
Console.WriteLine();
Console.WriteLine("New refresh token: {0}", tokens.RefreshToken);
Console.WriteLine();

Console.ReadLine();

static async Task CreateAccountAsync(HttpClient client, string email, string password)
{
    var response = await client.PostAsJsonAsync("https://localhost:44382/Account/Register", new { email, password });

    // Ignore 409 responses, as they indicate that the account already exists.
    if (response.StatusCode == HttpStatusCode.Conflict)
    {
        return;
    }

    response.EnsureSuccessStatusCode();
}

static async Task<(string AccessToken, string RefreshToken)> GetTokensAsync(HttpClient client, string email, string password)
{
    // Retrieve the OpenIddict server configuration document containing the endpoint URLs.
    var configuration = await client.GetDiscoveryDocumentAsync("https://localhost:44382/");
    if (configuration.IsError)
    {
        throw new Exception($"An error occurred while retrieving the configuration document: {configuration.Error}");
    }

    var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
    {
        Address = configuration.TokenEndpoint,
        UserName = email,
        Password = password,
        Scope = OidcConstants.StandardScopes.OfflineAccess
    });

    if (response.IsError)
    {
        throw new Exception($"An error occurred while retrieving an access token: {response.Error}");
    }

    return (response.AccessToken, response.RefreshToken);
}

static async Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync(HttpClient client, string token)
{
    // Retrieve the OpenIddict server configuration document containing the endpoint URLs.
    var configuration = await client.GetDiscoveryDocumentAsync("https://localhost:44382/");
    if (configuration.IsError)
    {
        throw new Exception($"An error occurred while retrieving the configuration document: {configuration.Error}");
    }

    var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
    {
        Address = configuration.TokenEndpoint,
        RefreshToken = token
    });

    if (response.IsError)
    {
        throw new Exception($"An error occurred while retrieving an access token: {response.Error}");
    }

    return (response.AccessToken, response.RefreshToken);
}
