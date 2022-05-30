using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityModel.OidcClient;

Console.WriteLine("Press any key to start the authentication process.");
Console.ReadKey();

// Create a local web server used to receive the authorization response.
using var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8739/");
listener.Start();

var options = new OidcClientOptions
{
    Authority = "https://localhost:44319/",
    ClientId = "console_app",
    LoadProfile = false,
    RedirectUri = "http://localhost:8739/",
    Scope = "openid api1 api2",
    IdentityTokenValidator = new JwtHandlerIdentityTokenValidator()
};

var client = new OidcClient(options);
var state = await client.PrepareLoginAsync(new Parameters(new Dictionary<string, string>
{
    ["hardcoded_identity_id"] = "1"
}));

// Launch the system browser to initiate the authentication dance.
Process.Start(new ProcessStartInfo
{
    FileName = state.StartUrl,
    UseShellExecute = true
});

// Wait for an authorization response to be posted to the local server.
while (true)
{
    var context = await listener.GetContextAsync();

    context.Response.ContentType = "text/plain";
    context.Response.StatusCode = 200;

    var buffer = Encoding.UTF8.GetBytes("Login completed. Please return to the console application.");
    await context.Response.OutputStream.WriteAsync(buffer);
    await context.Response.OutputStream.FlushAsync();

    context.Response.Close();

    var result = await client.ProcessResponseAsync(context.Request.Url.Query, state);
    if (result.IsError)
    {
        Console.WriteLine("An error occurred: {0}", result.Error);
    }

    else
    {
        Console.WriteLine("Response from Api1: {0}", await GetResourceFromApi1Async(result.AccessToken));
        Console.WriteLine("Response from Api2: {0}", await GetResourceFromApi2Async(result.AccessToken));
        break;
    }
}

Console.ReadLine();

static async Task<string> GetResourceFromApi1Async(string token)
{
    using var client = new HttpClient();

    using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44342/api");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
    {
        return "The user represented by the access token is not allowed to access Api1.";
    }

    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}

static async Task<string> GetResourceFromApi2Async(string token)
{
    using var client = new HttpClient();

    using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44379/api");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
    {
        return "The user represented by the access token is not allowed to access Api2.";
    }

    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}
