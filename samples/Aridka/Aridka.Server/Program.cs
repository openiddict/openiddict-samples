using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Aridka.Server.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure the context to use sqlite.
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-aridka-server.sqlite3")}");

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need
    // to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

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

        // Enable the client credentials flow.
        options.AllowClientCredentialsFlow();

        // Register the signing and encryption credentials.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Enable self_signed_tls_client_auth to allow clients to authenticate using self-signed certificates.
        options.EnableSelfSignedTlsClientAuthentication();

        // Note: setting a static issuer is mandatory when using mTLS aliases to ensure it not
        // dynamically computed based on the request URI, as this would result in two different
        // issuers being used (one pointing to the mTLS domain and one pointing to the regular one).
        options.SetIssuer("https://localhost:44385/");

        // Configure the mTLS endpoint aliases that will be used by client applications opting
        // for TLS-based client authentication to communicate with the authorization server:
        // the configured URIs MUST point to a domain for which the HTTPS server is configured
        // to require the use of client certificates when receiving TLS handshakes from clients.
        options.SetMtlsTokenEndpointAliasUri("https://mtls.dev.localhost:44385/connect/token");

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

// Configure Kestrel to listen on the 44385 port and configure it to enforce mTLS.
//
// Note: depending on the operating system, the mtls.dev.localhost
// subdomain MAY have to be manually mapped to 127.0.0.1 or ::1.
builder.Services.Configure<KestrelServerOptions>(options => options.ListenAnyIP(44385, options =>
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

    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    if (await manager.FindByClientIdAsync("console") == null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "console",
            DisplayName = "My client application",
            JsonWebKeySet = new JsonWebKeySet
            {
                Keys =
                {
                    // This application authenticates by using a self-signed client authentication
                    // certificate during the TLS handshake. While the client needs access to the
                    // private key, the server only needs to know the public part - included by the
                    // ExportCertificatePem() API - to be able to validate the certificates it receives.
                    JsonWebKeyConverter.ConvertFromX509SecurityKey(new X509SecurityKey(
                        X509Certificate2.CreateFromPem($"""
                            -----BEGIN CERTIFICATE-----
                            MIIE8zCCAtugAwIBAgIJAI/egicFvVmsMA0GCSqGSIb3DQEBCwUAMCIxIDAeBgNV
                            BAMTF1NlbGYtc2lnbmVkIGNlcnRpZmljYXRlMCAXDTI2MDMxMTE0MDkwMloYDzIx
                            MjYwMzExMTQwOTAyWjAiMSAwHgYDVQQDExdTZWxmLXNpZ25lZCBjZXJ0aWZpY2F0
                            ZTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAL6lxgz4nzb9B/ajfjy+
                            DKYJR3isyZ3kJg8f5BvKYFKptQTZWk4TJhChYzNrePBq7I/AW20z4Zt4TZvnAC29
                            z54EZ/9auxXswHN1tNujc1OPtADRlJmWYf/fI+y4cwoPegaPdSS+WRgeigz6BmJu
                            Oq5J6/IM/eRBqByaZgYU8Acvjxec9fzoKooK94ZoGeoM6q2GVipHzKLgPp4AsqA/
                            aY9Uz/sb6mjp/wQX4KcIFZLChGcYetuTkPuh6OcnCwApH67NvZB4HwxXDEMKx89V
                            JkUh8iOt+1Q3s76yfRl3JGcthrrhzojE/3teQpU3F/aKfou4Hagys3WXQv9q8V6m
                            s0rXEBF2WuXTF5kCdBhvCnFClrokm20ev8T4aSsI8FDXanMhTamHsFYE5eIgZXZy
                            q4fYcwnfHENGwvxp3w1tsdSqdtRXBUoJQYNbGyVsZeEADFlLERoqe6FRlnvZVtkZ
                            tThlYKUwWitOdivVuI5h+64HtJbhBvvdGPDVl3WQVDmQ4Z17HpyPCL6uILNlSxIu
                            Df+CdOZv3/iElSWUkR3WjO0qohv5TEraCwvhXlCyIK37WyzAXr7XVmZUrpmqU2aa
                            U4tVmt3o5zDorb7MCit9Mn731nfpjC6wSnHD90JGi7fSHUP7+GH0PfWNDSvjRtrY
                            bBThQA55vqVzcqHe5M6uCCwVAgMBAAGjKjAoMA4GA1UdDwEB/wQEAwIHgDAWBgNV
                            HSUBAf8EDDAKBggrBgEFBQcDAjANBgkqhkiG9w0BAQsFAAOCAgEAvCjsrSZQ1iKM
                            B42rDuE/IBbw5BUn12RzX608uG7CnTgiywwKSokspAi+3N7HEH4+8T+urQDQxCv8
                            aFZ4SpkDM8xXCh0zF9WIRK3kQYF+crNfrsCJaduCQjCozh1NyZe3oFTXxpHuKh7V
                            ellexvahLJid9a1bVADAIx5cKLEFhkSVh15hcWlphKMkVsA+cI0D22gbMwO2TSkL
                            +X2C0YQYd0yxrDjZKU2Y5P8vunDCMPS04UsexERRuUCRzFBp7+mt2c1rT0gxta9w
                            NeuW7ooUIAeMGNUY2FCrUI4OwMlre6knYZST+sfKyLY6r95PtHgXQB5pZ9G6iHu6
                            FGdqbvdqcqr79l989z6sOo7p4CzX3dxp3rAuBzgY023bTnNZAnEEYSNYd5AJePD2
                            1ycEXKEh1+GGkF0t5HX3FVe7VC/AEqCpNwaHzW0KQ6wunuqAJNvGa4gpZVqWGw7f
                            dBhkg+W5itWbAn3giXQQD8yi/0CJzBSj/GFVPWCax3n3dV404DTAqINq1Koix/1i
                            oOY+/PQEwlk+QZrtvBpPaDIjX7wVMnu7lF6q/d5gws3kHVPW90+8Nk/pXTeXU6mo
                            hO6dOCwfN1IrRSn1pQ35UsSKPE9/g5gN77hi0v9AK9jtPfLLJQGYvBAjlU7Y3p7R
                            Sq+yMXEPCIhq0DMdISeTf1Ajy7rYJcI=
                            -----END CERTIFICATE-----
                            """)))
                }
            },
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials
            }
        });
    }
}

await app.RunAsync();
