using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Client;
using static OpenIddict.Abstractions.OpenIddictConstants;

var services = new ServiceCollection();
services.AddOpenIddict()
    .AddClient(options =>
    {
        options.AddEphemeralEncryptionKey()
               .AddEphemeralSigningKey();

        options.DisableTokenStorage();

        options.UseSystemNetHttp();

        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:44382/", UriKind.Absolute)
        });
    });

await using var provider = services.BuildServiceProvider();

const string email = "bob@le-magnifique.com", password = "}s>EWG@f4g;_v7nB";

await CreateAccountAsync(provider, email, password);

var tokens = await GetTokensAsync(provider, email, password);
Console.WriteLine("Initial access token: {0}", tokens.AccessToken);
Console.WriteLine();
Console.WriteLine("Initial refresh token: {0}", tokens.RefreshToken);

Console.WriteLine();
Console.WriteLine();

tokens = await RefreshTokensAsync(provider, tokens.RefreshToken);
Console.WriteLine("New access token: {0}", tokens.AccessToken);
Console.WriteLine();
Console.WriteLine("New refresh token: {0}", tokens.RefreshToken);
Console.WriteLine();

Console.ReadLine();

static async Task CreateAccountAsync(IServiceProvider provider, string email, string password)
{
    using var client = provider.GetRequiredService<HttpClient>();
    var response = await client.PostAsJsonAsync("https://localhost:44382/Account/Register", new { email, password });

    // Ignore 409 responses, as they indicate that the account already exists.
    if (response.StatusCode == HttpStatusCode.Conflict)
    {
        return;
    }

    response.EnsureSuccessStatusCode();
}

static async Task<(string AccessToken, string RefreshToken)> GetTokensAsync(IServiceProvider provider, string email, string password)
{
    var service = provider.GetRequiredService<OpenIddictClientService>();

    var (response, _) = await service.AuthenticateWithPasswordAsync(
        issuer  : new Uri("https://localhost:44382/", UriKind.Absolute),
        scopes  : new[] { Scopes.OfflineAccess },
        username: email,
        password: password);

    return (response.AccessToken, response.RefreshToken);
}

static async Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync(IServiceProvider provider, string token)
{
    var service = provider.GetRequiredService<OpenIddictClientService>();

    var (response, _) = await service.AuthenticateWithRefreshTokenAsync(
        issuer: new Uri("https://localhost:44382/", UriKind.Absolute),
        token : token);

    return (response.AccessToken, response.RefreshToken);
}
