using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client;

var services = new ServiceCollection();

services.AddOpenIddict()

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Allow grant_type=client_credentials to be negotiated.
        options.AllowClientCredentialsFlow();

        // Disable token storage, which is not necessary for non-interactive flows like
        // grant_type=password, grant_type=client_credentials or grant_type=refresh_token.
        options.DisableTokenStorage();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Add a client registration matching the client application definition in the server project.
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:44385/", UriKind.Absolute),

            ClientId = "console",

            // Note: instead of sending a client secret, this application authenticates by using
            // a self-signed client authentication certificate during the TLS handshake.
            SigningCredentials = { GetSelfSignedCertificate() }
        });
    });

// Register a named HTTP client that will be used to call the demo resource API.
//
// Note: since the authorization server is configured to issue certificate-bound
// access tokens, the client certificate MUST be attached to outgoing HTTP requests
// and the mTLS subdomain (for which TLS client authentication is enabled) MUST be used.
services.AddHttpClient("ApiClient")
    .AddAsKeyed()
    .ConfigureHttpClient(static client => client.BaseAddress = new Uri("https://mtls.dev.localhost:44385/"))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ClientCertificateOptions = ClientCertificateOption.Manual,
        ClientCertificates = { GetSelfSignedCertificate().Certificate }
    });

await using var provider = services.BuildServiceProvider();

var token = await GetTokenAsync(provider);
Console.WriteLine("Access token: {0}", token);
Console.WriteLine();

var resource = await GetResourceAsync(provider, token);
Console.WriteLine("API response: {0}", resource);
Console.ReadLine();

static async Task<string> GetTokenAsync(IServiceProvider provider)
{
    var service = provider.GetRequiredService<OpenIddictClientService>();

    var result = await service.AuthenticateWithClientCredentialsAsync(new());
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

static X509SigningCredentials GetSelfSignedCertificate()
{
    // Note: OpenIddict only negotiates PKI-based or self-signed mutual
    // TLS authentication if the certificate explicitly contains the
    // "digitalSignature" key usage and the "clientAuth" extended key usage.
    var certificate = X509Certificate2.CreateFromPem(
        certPem: $"""
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
            """,
        keyPem: $"""
            -----BEGIN RSA PRIVATE KEY-----
            MIIJKAIBAAKCAgEAvqXGDPifNv0H9qN+PL4MpglHeKzJneQmDx/kG8pgUqm1BNla
            ThMmEKFjM2t48Grsj8BbbTPhm3hNm+cALb3PngRn/1q7FezAc3W026NzU4+0ANGU
            mZZh/98j7LhzCg96Bo91JL5ZGB6KDPoGYm46rknr8gz95EGoHJpmBhTwBy+PF5z1
            /Ogqigr3hmgZ6gzqrYZWKkfMouA+ngCyoD9pj1TP+xvqaOn/BBfgpwgVksKEZxh6
            25OQ+6Ho5ycLACkfrs29kHgfDFcMQwrHz1UmRSHyI637VDezvrJ9GXckZy2GuuHO
            iMT/e15ClTcX9op+i7gdqDKzdZdC/2rxXqazStcQEXZa5dMXmQJ0GG8KcUKWuiSb
            bR6/xPhpKwjwUNdqcyFNqYewVgTl4iBldnKrh9hzCd8cQ0bC/GnfDW2x1Kp21FcF
            SglBg1sbJWxl4QAMWUsRGip7oVGWe9lW2Rm1OGVgpTBaK052K9W4jmH7rge0luEG
            +90Y8NWXdZBUOZDhnXsenI8Ivq4gs2VLEi4N/4J05m/f+ISVJZSRHdaM7SqiG/lM
            StoLC+FeULIgrftbLMBevtdWZlSumapTZppTi1Wa3ejnMOitvswKK30yfvfWd+mM
            LrBKccP3QkaLt9IdQ/v4YfQ99Y0NK+NG2thsFOFADnm+pXNyod7kzq4ILBUCAwEA
            AQKCAgBgLvKEiMqKy43A+SsvKhLnkbblQwdVCU3KQ6SqAKgoDEavc5kD2tVRfpq1
            znrtkIRY4gs+RPaFoWRGS3zjluewKTjus6+/l/pgRfpA9W2xssZ1w0bdVemLVeCi
            BUzEvpopxSasqvv4FzA+68Vc04/3boQDUlqlVhqik6L1XoralTv0BdR1DAyqKG5I
            +SxZ0Lp1YVkHa8HqSohM3r0/674t+fQUFDlnRObMAd/tZT69FDYIbWlOblyvFziR
            pjj+k8DQSCxjPrcrWp9tE3tLNwJfzoiDR7uM+a1NgG9s8ZcEFwvqLRIuHnVmoF+n
            OGx2jdjaVMFhonK32OCMTEAKKMA7GqspNnHErKvxTsO4bb/cj0WG+6nuv0fN0IGc
            qK5LoO3+AH6pUttpQj/xPR5Xvpv54fme2AD8r2xqPGyiKjxAp8O1p3eDLsau1Dtd
            3PFuY/jiEGkRu+7HbmFnYDST9+ZZZ+du0/80M32jbN8+uDhQq7XXTph4GDaCA6tq
            v4GWF9JGKXiPiy0B5HzOrBZbncgutbqOKmX+5Cy/5vL6OhwT2el6KnFH8HNd7RSG
            0n+6cnaT0yIpmCnWZPeszZENMNvCWLoPS2qck9+KGNUEG6jnwDvqeWBAuqdTQsZD
            L9TQCXt5n3sdc2YkkfIG25dLd8riSwxjQdjrFAdTQxJMWngu4QKCAQEA0rSCsj4N
            4qIoPdcg+li/svoBYZMrJTT7CDgkk8gFKa8ZO9iwPDa5PHFfuq9N1pxNyLXfoZKe
            /Ev9TnRqy/m7vbYG1TrDLrZqmpNHTvcX9PoL4Mq+BM9fXDhKqU7slg1QBcq4qR5x
            QiTK7EtC65r12luTVUOcwbLRzjgqCdK4tedsYcPpeulj39wDzTQm5R1Gk0wrq/fc
            1pL5A2p6rv/VQojfjq/KmJsD+5J2lB6xhJBae2Bp1nSlRCaYC0S+cXvpwVhYy8Yw
            SNtCLVM/sGOgV+/zWeX9zSOCKrlDZSiEAhCKoQamkuJXF+C8+CbNCOBzdpS3/7U+
            8F9rk5W/5pfqiwKCAQEA56F1W6hu5ntHTxK4wryjoY8L4HJnr+f4RG9b+s1VaMeP
            y9bjO3Uuy3scKAWnDcDeSBPgFcXve8LMfscfkw0uYxZsq2AH07cnqDuqJP61jVaD
            mmDal0SLEfpciJNB9RznNcTyUviFEXHYAQFtuDpfE1gfkhU0fJgm4N164yKhDbQn
            GP4XNRn9FAX6SZJuQcmqRT8fMcHByYs3gHd15gf/w/Iuksi1Q2Tk5dbY4s3LRC3Q
            xQo//NRDJhn8gGnP2T3O4oS7813EHsSwlc1OKAHxsYZtWrscTPItxUUicvKYeqsk
            zJGADYZL1BPGVb00SGdw7uwkAkYAhQndVD9Ecc033wKCAQBplr/wJpy6t9xGsSn7
            isH2JMbQaPm0GYq7Ibdiv1em/fI9RWd7pUjKe14npXXyWD26mTnKNDmr4UC9MiXa
            tflZJoDiFiJ9pDhj4e5YKgc9YpjVO4Rh0LHO+v6fPcfdoio53M8RIQpMxTdTlpug
            ifUuSbnZfpptjvkIyKh4Z7rcnW54x76XM6IzKoRVLw9WvYcChadU9E8c0GYtSgzU
            6aurPgAZ9wol03j5dvopXABFmDlfnn8rUyUGs/h5nSd6o0gO9gD5jQXhXM8a+57s
            +9/8cWiX4mN/i43Nby3Q4a7VggiWjUioTviqJJtOF9Oj4Sa7g+d5IxC5UHgOa3rR
            ScvlAoIBAQDeKYQwd2p28cLBWsmPLfMb3+GaUuCUXT9IFC76bLsAlnebIO4tdwV8
            8QVedZ12mYgZRcbl20UJRRtydXYZSsk1DKsJ7D9VlxQYTbGxbgOgHlx3U3IVKA7j
            HWhnLiZS/HfeoJlzbx3iT3jH7iDYVFQgb6NIL8J5xk1z27oj5HDofeQKGpsTuWt9
            KwaWTjYmL1B6vkIjLR27OyXut6WDDiUIQV7eNld03m6U6+52CsBtEixs8JnS25vU
            DZSbbeGHEbs+k+TZVRPoFurvo0zVHpg8lxyHq3NHcfjofpi9+2S4MzJGaz+QuUA9
            lwHh9mkREPXGkwMukwmokH+ScGQrapOtAoIBADV+5ktQZIyPxX1SywAgsOQpNF6J
            ytbTRwVMcS9L2QrvH1wqLZs9ATYRHNTvbALdKHTKOi2ohaKufnG58Qss2lvB/UvI
            lJmSkLp9aJKP+6N5/YvjTOhGdRS2jo5jIZO1a27m34c965ivmCtF9aIYtzpni96z
            T8tIl9JodIjD6gXAzqpx1XRTnLes6DAmMh3J2ZztzDGSuJqhZgtFeyg4AR7sgCiP
            3Bbt+6G1U6jzJ2t85NIgHzWMnmrykhxU58nW2NFgCv8AdfEpO8gtPEQ832Mu3eKY
            /P3CcMl2yCoD786/YdSnsLnO+hD50Crc98YK19M1edCsJfTaL8RZzU63AGM=
            -----END RSA PRIVATE KEY-----
            """);

    // On Windows, a certificate loaded from PEM-encoded material is ephemeral and
    // cannot be directly used with TLS, as Schannel cannot access it in this case.
    //
    // To work this limitation, the certificate is exported and re-imported from a
    // PFX blob to ensure the private key is persisted in a way that Schannel can use.
    //
    // In a real world application, the certificate wouldn't be embedded in the source code
    // and would be installed in the certificate store, making this workaround unnecessary.
    if (OperatingSystem.IsWindows())
    {
        certificate = X509CertificateLoader.LoadPkcs12(
            data: certificate.Export(X509ContentType.Pfx, string.Empty),
            password: string.Empty,
            keyStorageFlags: X509KeyStorageFlags.DefaultKeySet);
    }

    return new X509SigningCredentials(certificate);
}
