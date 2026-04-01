using System.Globalization;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
builder.Services.AddQuartz(options =>
{
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddDbContext<DbContext>(options =>
{
    // Configure the context to use sqlite.
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-zirku-server.sqlite3")}");

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need
    // to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()

    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<DbContext>();
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        // Enable the authorization, introspection and token endpoints.
        options.SetAuthorizationEndpointUris("connect/authorize")
               .SetIntrospectionEndpointUris("connect/introspect")
               .SetTokenEndpointUris("connect/token");

        // Note: this sample only uses the authorization code and refresh token
        // flows but you can enable the other flows if you need to support implicit,
        // password or client credentials.
        options.AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow();

        // Register the encryption credentials. This sample uses a symmetric
        // encryption key that is shared between the server and the Api2 sample
        // (that performs local token validation instead of using introspection).
        //
        // Note: in a real world application, this encryption key should be
        // stored in a safe place (e.g in Azure KeyVault, stored as a secret).
        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        // Register the signing credentials.
        options.AddDevelopmentSigningCertificate();

        // Note: setting a static issuer is mandatory when using mTLS aliases to ensure it not
        // dynamically computed based on the request URI, as this would result in two different
        // issuers being used (one pointing to the mTLS domain and one pointing to the regular one).
        options.SetIssuer("https://localhost:44319/");

        // Enable self_signed_tls_client_auth to allow clients to use mTLS-based token binding.
        options.EnableSelfSignedTlsClientAuthentication();

        // Configure the mTLS endpoint aliases that will be used by client applications opting
        // for TLS-based client authentication to communicate with the authorization server:
        // the configured URIs MUST point to a domain for which the HTTPS server is configured
        // to require the use of client certificates when receiving TLS handshakes from clients.
        options.SetMtlsIntrospectionEndpointAliasUri("https://mtls.dev.localhost:44319/connect/introspect")
               .SetMtlsTokenEndpointAliasUri("https://mtls.dev.localhost:44319/connect/token");

        // While public client applications cannot use mTLS for client authentication, they can use
        // mTLS purely as a token binding mechanism: in this case, the refresh tokens issued to
        // public clients sending a client certificate are automatically bound to the certificate,
        // which requires sending the same certificate when using them to get new access tokens.
        options.UseClientCertificateBoundRefreshTokens();

        // Optionally, the server stack can be configured to issue client certificate-bound access tokens.
        //
        // When doing so, the standard "cnf" claim is automatically added to access tokens to inform
        // resource servers that a proof of possession derived from the certificate must be provided.
        options.UseClientCertificateBoundAccessTokens();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        //
        // Note: unlike other samples, this sample doesn't use token endpoint pass-through
        // to handle token requests in a custom MVC action. As such, the token requests
        // will be automatically handled by OpenIddict, that will reuse the identity
        // resolved from the authorization code to produce access and identity tokens.
        //
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough();
    });

// Configure Kestrel to listen on the 44319 port and configure it to enforce mTLS.
//
// Note: depending on the operating system, the mtls.dev.localhost
// subdomain MAY have to be manually mapped to 127.0.0.1 or ::1.
builder.Services.Configure<KestrelServerOptions>(options => options.ListenAnyIP(44319, options =>
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

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyHeader()
          .AllowAnyMethod()
          .WithOrigins("http://localhost:5112")));

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();

app.MapMethods("connect/authorize", [HttpMethods.Get, HttpMethods.Post], async (HttpContext context, IOpenIddictScopeManager manager) =>
{
    // Retrieve the OpenIddict server request from the HTTP context.
    var request = context.GetOpenIddictServerRequest() ??
        throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

    var identifier = (int?) request["hardcoded_identity_id"];
    if (identifier is not (1 or 2))
    {
        return Results.Challenge(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
            properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidRequest,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified hardcoded identity is invalid."
            }));
    }

    // Create the claims-based identity that will be used by OpenIddict to generate tokens.
    var identity = new ClaimsIdentity(
        authenticationType: TokenValidationParameters.DefaultAuthenticationType,
        nameType: Claims.Name,
        roleType: Claims.Role);

    // Add the claims that will be persisted in the tokens.
    identity.AddClaim(new Claim(Claims.Subject, identifier.Value.ToString(CultureInfo.InvariantCulture)));
    identity.AddClaim(new Claim(Claims.Name, identifier switch
    {
        1 => "Alice",
        2 => "Bob",
        _ => throw new InvalidOperationException()
    }));
    identity.AddClaim(new Claim(Claims.PreferredUsername, identifier switch
    {
        1 => "Alice",
        2 => "Bob",
        _ => throw new InvalidOperationException()
    }));

    // Note: in this sample, the client is granted all the requested scopes for the first identity (Alice)
    // but for the second one (Bob), only the "api1" scope can be granted, which will cause requests sent
    // to Zirku.Api2 on behalf of Bob to be automatically rejected by the OpenIddict validation handler,
    // as the access token representing Bob won't contain the "resource_server_2" audience required by Api2.
    identity.SetScopes(identifier switch
    {
        1 => request.GetScopes(),
        2 => new[] { Scopes.OpenId, "api1" }.Intersect(request.GetScopes()),
        _ => throw new InvalidOperationException()
    });

    identity.SetResources(await manager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

    // Allow all claims to be added in the access tokens.
    identity.SetDestinations(claim => [Destinations.AccessToken]);

    return Results.SignIn(new ClaimsPrincipal(identity), properties: null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
});

app.UseWelcomePage("/");

// Before starting the host, create the database used to store the application data.
//
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DbContext>();
    await context.Database.EnsureCreatedAsync();

    await CreateApplicationsAsync();
    await CreateScopesAsync();

    async Task CreateApplicationsAsync()
    {
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("console_app") is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ApplicationType = ApplicationTypes.Native,
                ClientId = "console_app",
                ClientType = ClientTypes.Public,
                RedirectUris =
                {
                    new Uri("http://localhost/")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                }
            };

            descriptor.AddScopePermissions("api1", "api2");

            await manager.CreateAsync(descriptor);
        }

        if (await manager.FindByClientIdAsync("spa") is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = "spa",
                ClientType = ClientTypes.Public,
                RedirectUris =
                {
                    new Uri("http://localhost:5112/index.html"),
                    new Uri("http://localhost:5112/signin-callback.html"),
                    new Uri("http://localhost:5112/signin-silent-callback.html"),
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.EndSession,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            };

            descriptor.AddScopePermissions("api1", "api2");

            await manager.CreateAsync(descriptor);
        }

        if (await manager.FindByClientIdAsync("resource_server_1") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "resource_server_1",
                JsonWebKeySet = new JsonWebKeySet
                {
                    Keys =
                    {
                        // Note: instead of sending a client secret, this application authenticates by
                        // generating client assertions that are signed using an ECDSA signing key.
                        //
                        // Note: while the client needs access to the private key, the server only needs
                        // to know the public key to be able to validate the client assertions it receives.
                        JsonWebKeyConverter.ConvertFromECDsaSecurityKey(GetECDsaSigningKey($"""
                            -----BEGIN PUBLIC KEY-----
                            MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAExEBWSim0vOd/397ejnxjXGhlMG8d
                            O+JAMsx3054Tuf/ogyvfhUE8COGfMZvKv5lcsyDw9YwwwJThZny5qs4vGw==
                            -----END PUBLIC KEY-----
                            """))
                    }
                },
                Permissions =
                {
                    Permissions.Endpoints.Introspection
                }
            });
        }

        // Note: no client registration is created for resource_server_2
        // as it uses local token validation instead of introspection.
    }

    async Task CreateScopesAsync()
    {
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        if (await manager.FindByNameAsync("api1") is null)
        {
            await manager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api1",
                Resources =
                {
                    "resource_server_1"
                }
            });
        }

        if (await manager.FindByNameAsync("api2") is null)
        {
            await manager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api2",
                Resources =
                {
                    "resource_server_2"
                }
            });
        }
    }
}

await app.RunAsync();

static ECDsaSecurityKey GetECDsaSigningKey(ReadOnlySpan<char> key)
{
    var algorithm = ECDsa.Create();
    algorithm.ImportFromPem(key);

    return new ECDsaSecurityKey(algorithm);
}
