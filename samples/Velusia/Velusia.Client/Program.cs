using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client;
using Quartz;
using Velusia.Client.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure the context to use sqlite.
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-velusia-client.sqlite3")}");

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need
    // to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})

.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
    options.SlidingExpiration = false;
});

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations from the database) at regular intervals.
builder.Services.AddQuartz(options =>
{
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddOpenIddict()

    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the Entity Framework Core stores and models.
        // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();

        // Developers who prefer using MongoDB can remove the previous lines
        // and configure OpenIddict to use the specified MongoDB database:
        // options.UseMongoDb()
        //        .UseDatabase(new MongoClient().GetDatabase("openiddict"));

        // Enable Quartz.NET integration.
        options.UseQuartz();
    })

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Note: this sample uses the code flow, but you can enable the other flows if necessary.
        options.AllowAuthorizationCodeFlow();

        // Register the signing and encryption credentials used to protect
        // sensitive data like the state tokens produced by OpenIddict.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
               .EnableStatusCodePagesIntegration()
               .EnableRedirectionEndpointPassthrough()
               .EnablePostLogoutRedirectionEndpointPassthrough();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Add a client registration matching the client application definition in the server project.
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:44313/", UriKind.Absolute),

            ClientId = "mvc",
            Scopes = { Scopes.Email, Scopes.Profile },

            // Note: instead of sending a client secret, this application authenticates by
            // generating client assertions that are signed using an ECDSA signing key.
            SigningCredentials =
            {
                new SigningCredentials(GetECDsaSigningKey($"""
                    -----BEGIN EC PRIVATE KEY-----
                    MHcCAQEEINEo+oyUvdFzYmEAB/z5x3loRAkMl/r/oyjko+RiVJ8KoAoGCCqGSM49
                    AwEHoUQDQgAEhlS5driPAQZJ3GnRMeEF+d9BBBzq/3nv8HxzS9zjgO26jPNCNYCT
                    hpeJ5l6TbyhbxlYligILa6Dt+iz074n/JA==
                    -----END EC PRIVATE KEY-----
                    """), SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.Sha256)
            },

            // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
            // URI per provider, unless all the registered providers support returning a special "iss"
            // parameter containing their URL as part of authorization responses. For more information,
            // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
            RedirectUri = new Uri("callback/login/local", UriKind.Relative),
            PostLogoutRedirectUri = new Uri("callback/logout/local", UriKind.Relative)
        });
    });

// Register a named HTTP client that will be used to call the demo resource API.
builder.Services.AddHttpClient("ApiClient")
    .AddAsKeyed()
    .ConfigureHttpClient(static client => client.BaseAddress = new Uri("https://localhost:44313/"));

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

// Before starting the host, create the database used to store the application data.
//
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

await app.RunAsync();

static ECDsaSecurityKey GetECDsaSigningKey(ReadOnlySpan<char> key)
{
    var algorithm = ECDsa.Create();
    algorithm.ImportFromPem(key);

    return new ECDsaSecurityKey(algorithm);
}
