using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

using var client = new HttpClient();

var token = await GetTokenAsync(client, "alice@wonderland.com", "P@ssw0rd");
Console.WriteLine("Access token: {0}", token);
Console.WriteLine();

var resource = await GetResourceAsync(client, token);
Console.WriteLine("API response: {0}", resource);

Console.ReadLine();

static async Task<string> GetTokenAsync(HttpClient client, string email, string password)
{
    var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:58779/connect/token");
    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["grant_type"] = "password",
        ["username"] = email,
        ["password"] = password
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
    var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:58779/api/message");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}
