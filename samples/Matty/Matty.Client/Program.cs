using Matty.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIddict.Client;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddDebug();

builder.Services.AddOpenIddict()

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Note: this sample uses the device authorization flow,
        // but you can enable the other flows if necessary.
        options.AllowDeviceAuthorizationFlow();

        // Disable token storage, which is not necessary for the device authorization flow.
        options.DisableTokenStorage();

        // Register the signing and encryption credentials used to protect
        // sensitive data like the state tokens produced by OpenIddict.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Add a client registration matching the client application definition in the server project.
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:44321/", UriKind.Absolute),

            ClientId = "device",
            Scopes = { Scopes.Email, Scopes.Profile, Scopes.OfflineAccess }
        });
    });

// Register the background service responsible for handling the console interactions.
builder.Services.AddHostedService<InteractiveService>();

var app = builder.Build();
await app.RunAsync();
