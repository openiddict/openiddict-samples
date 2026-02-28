using Dapplo.Microsoft.Extensions.Hosting.WinForms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Sorgan.WinForms.Client;

ApplicationConfiguration.Initialize();

var host = new HostBuilder()
    // Note: applications for which a single instance is preferred can reference
    // the Dapplo.Microsoft.Extensions.Hosting.AppServices package and call this
    // method to automatically close extra instances based on the specified identifier:
    //
    // .ConfigureSingleInstance(options => options.MutexId = "{7113F751-8CD1-42D8-B294-E5F360497577}")
    //
    .ConfigureLogging(options => options.AddDebug())
    .ConfigureServices(services =>
    {
        services.AddDbContext<DbContext>(options =>
        {
            options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-sorgan-winforms-client.sqlite3")}");
            options.UseOpenIddict();
        });

        services.AddOpenIddict()

            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                // Configure OpenIddict to use the Entity Framework Core stores and models.
                // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                options.UseEntityFrameworkCore()
                       .UseDbContext<DbContext>();
            })

            // Register the OpenIddict client components.
            .AddClient(options =>
            {
                // Note: this sample uses the authorization code and refresh token
                // flows, but you can enable the other flows if necessary.
                options.AllowAuthorizationCodeFlow()
                       .AllowRefreshTokenFlow();

                // Register the signing and encryption credentials used to protect
                // sensitive data like the state tokens produced by OpenIddict.
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // Add the operating system integration.
                options.UseSystemIntegration();

                // Register the System.Net.Http integration and use the identity of the current
                // assembly as a more specific user agent, which can be useful when dealing with
                // providers that use the user agent as a way to throttle requests (e.g Reddit).
                options.UseSystemNetHttp()
                       .SetProductInformation(typeof(Program).Assembly);

                // Register the Web providers integrations.
                //
                // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
                // address per provider, unless all the registered providers support returning an "iss"
                // parameter containing their URL as part of authorization responses. For more information,
                // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
                options.UseWebProviders()
                       .AddGitHub(options =>
                       {
                           options.SetClientId("fa9321227d63cda3f341")
                                  // Note: GitHub doesn't allow creating public clients and requires using a client secret.
                                  .SetClientSecret("d904b9b9ededc39da499b2ea4c13df5c7e35ddbe")
                                  // Note: GitHub doesn't support the recommended ":/" syntax and requires using "://".
                                  .SetRedirectUri("com.openiddict.sorgan.winforms.client://callback/login/github");
                       });
            });
    })
    .ConfigureWinForms<MainForm>()
    .UseWinFormsLifetime()
    .Build();

// Before starting the host, create the database used to store the application data
// and add the registry entries required to register the custom URI scheme.
//
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = host.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DbContext>();
    await context.Database.EnsureCreatedAsync();

    // Create the registry entries necessary to handle URI protocol activations.
    //
    // Note: this sample creates the entry under the current user account (as it doesn't
    // require administrator rights), but the registration can also be added globally
    // in HKEY_CLASSES_ROOT (in this case, it should be added by a dedicated installer).
    //
    // Alternatively, the application can be packaged and use windows.protocol to
    // register the protocol handler/custom URI scheme with the operation system.
    using var root = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\com.openiddict.sorgan.winforms.client");
    root.SetValue(string.Empty, "URL:com.openiddict.sorgan.winforms.client");
    root.SetValue("URL Protocol", string.Empty);

    using var command = root.CreateSubKey("shell\\open\\command");
    command.SetValue(string.Empty, string.Format("\"{0}\" \"%1\"", Environment.ProcessPath));
}

await host.RunAsync();
