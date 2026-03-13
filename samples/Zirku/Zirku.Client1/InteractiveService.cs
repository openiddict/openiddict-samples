using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Hosting;
using OpenIddict.Client;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Abstractions.OpenIddictExceptions;

namespace Zirku.Client1;

public class InteractiveService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly OpenIddictClientService _service;

    public InteractiveService(
        IHostApplicationLifetime lifetime,
        OpenIddictClientService service)
    {
        _lifetime = lifetime;
        _service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the host to confirm that the application has started.
        var source = new TaskCompletionSource<bool>();
        using (_lifetime.ApplicationStarted.Register(static state => ((TaskCompletionSource<bool>) state!).SetResult(true), source))
        {
            await source.Task;
        }

        Console.WriteLine("Press any key to start the authentication process.");
        await Task.Run(Console.ReadKey).WaitAsync(stoppingToken);

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

        try
        {
            // Ask OpenIddict to initiate the authentication flow (typically, by starting the system browser).
            var result = await _service.ChallengeInteractivelyAsync(new()
            {
                AdditionalAuthorizationRequestParameters = new()
                {
                    ["hardcoded_identity_id"] = "1"
                },
                CancellationToken = stoppingToken
            });

            Console.WriteLine("System browser launched.");

            // Wait for the user to complete the authorization process.
            var response = await _service.AuthenticateInteractivelyAsync(new()
            {
                Nonce = result.Nonce,
                TokenBindingCertificate = certificate
            });

            // Note: since the authorization server is configured to issue certificate-bound
            // access tokens, the client certificate MUST be attached to outgoing HTTP requests
            // and the mTLS subdomain (for which TLS client authentication is enabled) MUST be used.
            using var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ClientCertificates = { certificate }
            };

            Console.WriteLine("Response from Api1: {0}", await GetResourceFromApi1Async(
                (response.BackchannelAccessToken ?? response.FrontchannelAccessToken)!, handler, stoppingToken));
            Console.WriteLine("Response from Api2: {0}", await GetResourceFromApi2Async(
                (response.BackchannelAccessToken ?? response.FrontchannelAccessToken)!, handler, stoppingToken));
        }

        catch (OperationCanceledException)
        {
            Console.WriteLine("The authentication process was aborted.");
        }

        catch (ProtocolException exception) when (exception.Error is Errors.AccessDenied)
        {
            Console.WriteLine("The authorization was denied by the end user.");
        }

        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Forbidden)
        {
            Console.WriteLine("The user is not allowed to perform the requested action.");
        }

        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized)
        {
            Console.WriteLine("The access token is invalid, has expired or has been revoked.");
        }

        catch
        {
            Console.WriteLine("An error occurred while trying to authenticate the user.");
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

        async Task<string> GetResourceFromApi1Async(string token, HttpClientHandler handler, CancellationToken cancellationToken)
        {
            using var client = new HttpClient(handler, disposeHandler: false)
            {
                BaseAddress = new Uri("https://mtls.dev.localhost:44342/", UriKind.Absolute)
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, "api");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        async Task<string> GetResourceFromApi2Async(string token, HttpClientHandler handler, CancellationToken cancellationToken)
        {
            using var client = new HttpClient(handler, disposeHandler: false)
            {
                BaseAddress = new Uri("https://mtls.dev.localhost:44379/", UriKind.Absolute)
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, "api");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

    }
}
