using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Client;

// Note: the OpenIddict server stack supports mTLS-based token binding for public clients:
// while these clients cannot authenticate using a TLS client certificate, the certificate
// can be used to bind the refresh (and access) tokens returned by the authorization server
// to the client application, which prevents such tokens from being used without providing a
// proof-of-possession matching the TLS client certificate used when the token was acquired.
//
// While this sample deliberately doesn't store the generated certificate in a persistent
// location, the certificate used for token binding should typically be stored in the user
// certificate store to be reloaded across application restarts in a real-world application.
var certificate = GenerateEphemeralTlsClientCertificate();

var services = new ServiceCollection();

services.AddOpenIddict()

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Allow grant_type=password to be negotiated.
        options.AllowPasswordFlow();

        // Disable token storage, which is not necessary for non-interactive flows like
        // grant_type=password, grant_type=client_credentials or grant_type=refresh_token.
        options.DisableTokenStorage();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Add a client registration without a client identifier/secret attached.
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:44360/", UriKind.Absolute)
        });
    });

// Register a named HTTP client that will be used to call the demo resource API.
//
// Note: since the authorization server is configured to issue certificate-bound
// access tokens, the client certificate MUST be attached to outgoing HTTP requests
// and the mTLS subdomain (for which TLS client authentication is enabled) MUST be used.
services.AddHttpClient("ApiClient")
    .AddAsKeyed()
    .ConfigureHttpClient(static client => client.BaseAddress = new Uri("https://mtls.dev.localhost:44360/"))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ClientCertificateOptions = ClientCertificateOption.Manual,
        ClientCertificates = { certificate }
    });

await using var provider = services.BuildServiceProvider();

const string email = "bob@le-magnifique.com", password = "}s>EWG@f4g;_v7nB";

await CreateAccountAsync(provider, email, password);

var token = await GetTokenAsync(provider, email, password, certificate);
Console.WriteLine("Access token: {0}", token);
Console.WriteLine();

var resource = await GetResourceAsync(provider, token);
Console.WriteLine("API response: {0}", resource);

Console.ReadLine();

static async Task CreateAccountAsync(IServiceProvider provider, string email, string password)
{
    var client = provider.GetRequiredKeyedService<HttpClient>("ApiClient");
    var response = await client.PostAsJsonAsync("Account/Register", new { email, password });

    // Ignore 409 responses, as they indicate that the account already exists.
    if (response.StatusCode == HttpStatusCode.Conflict)
    {
        return;
    }

    response.EnsureSuccessStatusCode();
}

static X509Certificate2 GenerateEphemeralTlsClientCertificate()
{
    using var algorithm = RSA.Create(keySizeInBits: 4096);

    var subject = new X500DistinguishedName("CN=Self-signed certificate");
    var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));
    request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.2")], critical: true));

    var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(2));

    // On Windows, a certificate loaded from PEM-encoded material is ephemeral and
    // cannot be directly used with TLS, as Schannel cannot access it in this case.
    //
    // To work this limitation, the certificate is exported and re-imported from a
    // PFX blob to ensure the private key is persisted in a way that Schannel can use.
    //
    // In a real world application, the certificate wouldn't be embedded in the source code
    // and would be installed in the certificate store, making this workaround unnecessary.
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        certificate = X509CertificateLoader.LoadPkcs12(
            data: certificate.Export(X509ContentType.Pfx, string.Empty),
            password: string.Empty,
            keyStorageFlags: X509KeyStorageFlags.DefaultKeySet);
    }

    return certificate;
}

static async Task<string> GetTokenAsync(IServiceProvider provider, string email, string password, X509Certificate2 certificate)
{
    var service = provider.GetRequiredService<OpenIddictClientService>();

    var result = await service.AuthenticateWithPasswordAsync(new()
    {
        Username = email,
        Password = password,
        TokenBindingCertificate = certificate
    });

    return result.AccessToken;
}

static async Task<string> GetResourceAsync(IServiceProvider provider, string token)
{
    var client = provider.GetRequiredKeyedService<HttpClient>("ApiClient");
    using var request = new HttpRequestMessage(HttpMethod.Get, "api/message");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}
