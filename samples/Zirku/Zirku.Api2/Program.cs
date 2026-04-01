using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register the OpenIddict validation components.
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Note: the validation handler uses OpenID Connect discovery
        // to retrieve the issuer signing keys used to validate tokens.
        options.SetIssuer("https://localhost:44319/");
        options.AddAudiences("resource_server_2");

        // Register the encryption credentials. This sample uses a symmetric
        // encryption key that is shared between the server and the Api2 sample
        // (that performs local token validation instead of using introspection).
        //
        // Note: in a real world application, this encryption key should be
        // stored in a safe place (e.g in Azure KeyVault, stored as a secret).
        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        // Register the System.Net.Http integration.
        options.UseSystemNetHttp();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyHeader()
          .AllowAnyMethod()
          .WithOrigins("http://localhost:5112")));

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

// Configure Kestrel to listen on the 44379 port and configure it to enforce mTLS.
//
// Note: depending on the operating system, the mtls.dev.localhost
// subdomain MAY have to be manually mapped to 127.0.0.1 or ::1.
builder.Services.Configure<KestrelServerOptions>(options => options.ListenAnyIP(44379, options =>
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

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("api", (ClaimsPrincipal user) => $"{user.Identity!.Name} is allowed to access Api2.").RequireAuthorization();

app.UseWelcomePage("/");

app.Run();
