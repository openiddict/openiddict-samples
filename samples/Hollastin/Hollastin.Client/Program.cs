using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Client;

var services = new ServiceCollection();

services.AddOpenIddict()

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Allow grant_type=password to be negotiated.
        options.AllowPasswordFlow();

        // Disable token storage, which is not necessary for non-interactive flows like
        // grant_type=password, grant_type=client_credentials or grant_type=refresh_token.
        options.DisableTokenStorage();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Add a client registration without a client identifier/secret attached.
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:44360/", UriKind.Absolute)
        });
    });

await using var provider = services.BuildServiceProvider();

const string email = "bob@le-magnifique.com", password = "}s>EWG@f4g;_v7nB";

await CreateAccountAsync(provider, email, password);

var token = await GetTokenAsync(provider, email, password);
Console.WriteLine("Access token: {0}", token);
Console.WriteLine();

var resource = await GetResourceAsync(provider, token);
Console.WriteLine("API response: {0}", resource);

Console.ReadLine();

static async Task CreateAccountAsync(IServiceProvider provider, string email, string password)
{
    using var client = provider.GetRequiredService<HttpClient>();
    var response = await client.PostAsJsonAsync("https://localhost:44360/Account/Register", new { email, password });

    // Ignore 409 responses, as they indicate that the account already exists.
    if (response.StatusCode == HttpStatusCode.Conflict)
    {
        return;
    }

    response.EnsureSuccessStatusCode();
}

static async Task<string> GetTokenAsync(IServiceProvider provider, string email, string password)
{
    var service = provider.GetRequiredService<OpenIddictClientService>();

    var (response, _) = await service.AuthenticateWithPasswordAsync(
        issuer  : new Uri("https://localhost:44360/", UriKind.Absolute),
        username: email,
        password: password);

    return response.AccessToken;
}

static async Task<string> GetResourceAsync(IServiceProvider provider, string token)
{
    using var client = provider.GetRequiredService<HttpClient>();
    using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44360/api/message");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}
