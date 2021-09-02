using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using IdentityModel.OidcClient;
using static IdentityModel.OidcConstants;

Console.WriteLine("Press any key to start the authentication process.");
Console.ReadKey();

// Create a local web server used to receive the authorization response.
using var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:7891/");
listener.Start();

var options = new OidcClientOptions
{
    Authority = "https://localhost:44387/",
    ClientId = "console_app",
    LoadProfile = false,
    RedirectUri = "http://localhost:7891/",
    Scope = StandardScopes.OpenId,
    IdentityTokenValidator = new JwtHandlerIdentityTokenValidator()
};

var client = new OidcClient(options);
var state = await client.PrepareLoginAsync();

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
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Claims:");

        foreach (var claim in result.User.Claims)
        {
            Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
        }

        Console.WriteLine();
        Console.WriteLine("Access token:");
        Console.WriteLine();
        Console.WriteLine(result.AccessToken);
        break;
    }
}

Console.ReadLine();