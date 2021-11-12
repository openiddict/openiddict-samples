using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using var client = new HttpClient();

try
{
    var token = await GetTokenAsync(client);
    Console.WriteLine("Access token: {0}", token);
    Console.WriteLine();

    var resource = await GetResourceAsync(client, token);
    Console.WriteLine("API response: {0}", resource);
    Console.ReadLine();
}
catch (HttpRequestException exception)
{
    var builder = new StringBuilder();
    builder.AppendLine("+++++++++++++++++++++");
    builder.AppendLine(exception.Message);
    builder.AppendLine(exception.InnerException?.Message);
    builder.AppendLine("Make sure you started the authorization server.");
    builder.AppendLine("+++++++++++++++++++++");
    Console.WriteLine(builder.ToString());
}

static async Task<string> GetTokenAsync(HttpClient client)
{
    var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44385/connect/token");
    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["grant_type"] = "client_credentials",
        ["client_id"] = "console",
        ["client_secret"] = "388D45FA-B36B-4988-BA59-B187D329C207"
    });

    var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);

    var payload = await response.Content.ReadFromJsonAsync<OpenIddictResponse>();

    if (!string.IsNullOrEmpty(payload.Error))
    {
        throw new InvalidOperationException("An error occurred while retrieving an access token.");
    }

    return payload.AccessToken;
}

static async Task<string> GetResourceAsync(HttpClient client, string token)
{
    var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44385/api/message");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}
