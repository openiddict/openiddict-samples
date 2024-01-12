using System.IO;
using System.Windows;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sorgan.BlazorHybrid.Client;

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

var host = new HostBuilder()
    // Note: applications for which a single instance is preferred can reference
    // the Dapplo.Microsoft.Extensions.Hosting.AppServices package and call this
    // method to automatically close extra instances based on the specified identifier:
    //
    // .ConfigureSingleInstance(options => options.MutexId = "{6FBAFC6B-528A-4CB7-A99A-B5DDF5812943}")
    //
    .ConfigureLogging(options => options.AddDebug())
    .ConfigureServices(services =>
    {
        services.AddDbContext<DbContext>(options =>
        {
            options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-sorgan-blazorhybrid-client.sqlite3")}");
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
                           options.SetClientId("2388b26eab831adab80d")
                                  // Note: GitHub doesn't allow creating public clients and requires using a client secret.
                                  .SetClientSecret("5115eeb4c840aeaaa19f7be7ea8b13b992dca765")
                                  // Note: GitHub doesn't support the recommended ":/" syntax and requires using "://", but allows
                                  // using a dynamic/random port that will be dynamically chosen by the OpenIddict system integration.
                                  .SetRedirectUri("http://localhost/callback/login/github");
                       });
            });

        // Register the worker responsible for creating the database used to store tokens
        // and adding the registry entries required to register the custom URI scheme.
        //
        // Note: in a real world application, this step should be part of a setup script.
        services.AddHostedService<Worker>();

        services.AddWpfBlazorWebView();
    })
    .ConfigureWpf(options =>
    {
        options.UseApplication<App>();
        options.UseWindow<MainWindow>();
    })
    .UseWpfLifetime()
    .Build();

await host.RunAsync();