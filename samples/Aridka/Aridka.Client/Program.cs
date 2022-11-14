using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Client;

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
            Issuer = new Uri("https://localhost:44385/", UriKind.Absolute),

            ClientId = "console",
            ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207"
        });
    });

await using var provider = services.BuildServiceProvider();

var token = await GetTokenAsync(provider);
Console.WriteLine("Access token: {0}", token);
Console.WriteLine();

var resource = await GetResourceAsync(provider, token);
Console.WriteLine("API response: {0}", resource);
Console.ReadLine();

static async Task<string> GetTokenAsync(IServiceProvider provider)
{
    var service = provider.GetRequiredService<OpenIddictClientService>();

    var (response, _) = await service.AuthenticateWithClientCredentialsAsync(new Uri("https://localhost:44385/", UriKind.Absolute));
    return response.AccessToken;
}

static async Task<string> GetResourceAsync(IServiceProvider provider, string token)
{
    using var client = provider.GetRequiredService<HttpClient>();
    using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44385/api/message");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}
