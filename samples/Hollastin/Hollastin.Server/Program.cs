using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Hollastin.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure the context to use sqlite.
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-hollastin-server.sqlite3")}");

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need
    // to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

// Register the Identity services.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
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

        // Enable Quartz.NET integration.
        options.UseQuartz();
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        // Enable the token endpoint.
        options.SetTokenEndpointUris("connect/token");

        // Enable the password flow.
        options.AllowPasswordFlow();

        // Accept anonymous clients (i.e clients that don't send a client_id).
        options.AcceptAnonymousClients();

        // Register the signing and encryption credentials.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Note: setting a static issuer is mandatory when using mTLS aliases to ensure it not
        // dynamically computed based on the request URI, as this would result in two different
        // issuers being used (one pointing to the mTLS domain and one pointing to the regular one).
        options.SetIssuer("https://localhost:44360/");

        // Enable self_signed_tls_client_auth to allow clients to use mTLS-based token binding.
        options.EnableSelfSignedTlsClientAuthentication();

        // Configure the mTLS endpoint aliases that will be used by client applications opting
        // for TLS-based client authentication to communicate with the authorization server:
        // the configured URIs MUST point to a domain for which the HTTPS server is configured
        // to require the use of client certificates when receiving TLS handshakes from clients.
        options.SetMtlsTokenEndpointAliasUri("https://mtls.dev.localhost:44360/connect/token");

        // Optionally, the server stack can be configured to issue client certificate-bound access tokens.
        //
        // When doing so, the standard "cnf" claim is automatically added to access tokens to inform
        // resource servers that a proof of possession derived from the certificate must be provided.
        options.UseClientCertificateBoundAccessTokens();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough();
    })

    // Register the OpenIddict validation components.
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

// Configure Kestrel to listen on the 44360 port and configure it to enforce mTLS.
//
// Note: depending on the operating system, the mtls.dev.localhost
// subdomain MAY have to be manually mapped to 127.0.0.1 or ::1.
builder.Services.Configure<KestrelServerOptions>(options => options.ListenAnyIP(44360, options =>
{
    options.UseHttps(new TlsHandshakeCallbackOptions
    {
        OnConnection = static context =>
        {
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            return ValueTask.FromResult(new SslServerAuthenticationOptions
            {
                // Require a client certificate for all the requests pointing to the mTLS subdomain.
                ClientCertificateRequired = string.Equals(context.ClientHelloInfo.ServerName,
                    "mtls.dev.localhost", StringComparison.OrdinalIgnoreCase),

                // Ignore all the client certificate errors for requests pointing to
                // the mTLS-specific domain, even if they indicate that the chain is
                // invalid: this is necessary to allow OpenIddict to validate the PKI
                // and self-signed certificates using its own per-client chain policies.
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) =>
                {
                    if (string.Equals(context.ClientHelloInfo.ServerName,
                        "mtls.dev.localhost", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    return errors is SslPolicyErrors.None or SslPolicyErrors.RemoteCertificateNotAvailable;
                },

                // Use the development certificate generated and stored by ASP.NET Core in the user store.
                ServerCertificate = store.Certificates
                    .Find(X509FindType.FindByExtension, "1.3.6.1.4.1.311.84.1.1", validOnly: false)
                    .Cast<X509Certificate2>()
                    .Where(static certificate => certificate.NotBefore < TimeProvider.System.GetLocalNow())
                    .Where(static certificate => certificate.NotAfter > TimeProvider.System.GetLocalNow())
                    .OrderByDescending(static certificate => certificate.NotAfter)
                    .FirstOrDefault() ??
                    throw new InvalidOperationException("The ASP.NET Core HTTPS development certificate was not found.")
            });
        }
    });
}));

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

app.UseWelcomePage("/");

// Before starting the host, create the database used to store the application data.
//
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

await app.RunAsync();
