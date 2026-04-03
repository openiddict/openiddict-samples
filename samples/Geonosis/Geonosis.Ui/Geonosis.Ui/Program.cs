using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using Geonosis.Ui.Client.Weather;
using Geonosis.Ui.Components;
using Geonosis.Ui.Weather;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using Yarp.ReverseProxy.Transforms;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-geonosis-ui.sqlite3")}");
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()

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
        // Enable the authorization code flow, the refresh token flow and the token exchange flow.
        options.AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow()
               .AllowTokenExchangeFlow();

        // Register the signing and encryption credentials used to protect
        // sensitive data like the state tokens produced by OpenIddict.
        // TODO: add custom certificates in production scenarios.
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
            Issuer = new Uri("https://localhost:7094/", UriKind.Absolute),

            ClientId = "geonosis-ui",

            // Note: instead of sending a client secret, this application authenticates by
            // generating client assertions that are signed using an ECDSA signing key.
            SigningCredentials =
            {
                new SigningCredentials(GetECDsaSigningKey($"""
                    -----BEGIN EC PRIVATE KEY-----
                    MHcCAQEEIAySayLGEX8781cE7W8HaJsNTqb9Ucym6SApQgIVdFZvoAoGCCqGSM49
                    AwEHoUQDQgAEFXmvZRv1zOogKS8JP/qlGxNC+GhrUpYIGykTeHPrvrY3HFpHnQ7h
                    vNQzLULWxLkuzsu95cMzJIuITdr7e1i8cg==
                    -----END EC PRIVATE KEY-----
                    """), SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.Sha256)
            },

            Scopes = { Scopes.OfflineAccess, Scopes.Email, Scopes.Profile, Scopes.Roles },

            // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
            // URI per provider, unless all the registered providers support returning a special "iss"
            // parameter containing their URL as part of authorization responses. For more information,
            // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
            RedirectUri = new Uri("authentication/login-callback/local", UriKind.Relative),
            PostLogoutRedirectUri = new Uri("authentication/logout-callback/local", UriKind.Relative)
        });
    });

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/authentication/login";
        options.LogoutPath = "/authentication/logout";
    });

builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpForwarderWithServiceDiscovery();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IWeatherForecaster, ServerWeatherForecaster>(client =>
{
    client.BaseAddress = new("https://localhost:7070/");
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Geonosis.Ui.Client._Imports).Assembly);

// Register the endpoint responsible for redirecting the user to the authorization endpoint of the identity provider.
app.MapGet("/authentication/login", (string? returnUrl) =>
{
    var properties = new AuthenticationProperties
    {
        // Only allow local return URLs to prevent open redirect attacks.
        RedirectUri = RedirectHttpResult.IsLocalUrl(returnUrl) ? returnUrl : "/"
    };

    return TypedResults.Challenge(properties, [OpenIddictClientAspNetCoreDefaults.AuthenticationScheme]);
}).AllowAnonymous();

// Register the endpoint responsible for redirecting the user to the end session endpoint of the identity provider.
app.MapPost("/authentication/logout", ([FromForm] string? returnUrl, HttpContext context) =>
{
    var properties = new AuthenticationProperties
    {
        // Only allow local return URLs to prevent open redirect attacks.
        RedirectUri = RedirectHttpResult.IsLocalUrl(returnUrl) ? returnUrl : "/"
    };

    return TypedResults.SignOut(properties, [CookieAuthenticationDefaults.AuthenticationScheme, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme]);
}).AllowAnonymous();

// Register the endpoint responsible for handling the authorization response returned by the identity provider.
app.MapMethods("/authentication/login-callback/{provider}", [HttpMethods.Get, HttpMethods.Post], async (string? provider, HttpContext context) =>
{
    var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    if (result is not { Succeeded: true, Principal.Identity.IsAuthenticated: true })
    {
        throw new InvalidOperationException("The external authorization data cannot be used for authentication.");
    }

    // Build an identity based on the external claims and that will be used to create the authentication cookie.
    var identity = new ClaimsIdentity(
        authenticationType: "ExternalLogin",
        nameType: ClaimTypes.Name,
        roleType: ClaimTypes.Role);

    // By default, OpenIddict will automatically try to map the email/name and name identifier claims from
    // their standard OpenID Connect or provider-specific equivalent, if available. If needed, additional
    // claims can be resolved from the external identity and copied to the final authentication cookie.
    identity.SetClaim(ClaimTypes.Email, result.Principal.GetClaim(ClaimTypes.Email))
            .SetClaim(ClaimTypes.Name, result.Principal.GetClaim(ClaimTypes.Name))
            .SetClaim(ClaimTypes.NameIdentifier, result.Principal.GetClaim(ClaimTypes.NameIdentifier))
            .SetClaim(ClaimTypes.Role, result.Principal.GetClaim("Role"));

    // Preserve the registration details to be able to resolve them later.
    identity.SetClaim(Claims.Private.RegistrationId, result.Principal.GetClaim(Claims.Private.RegistrationId))
            .SetClaim(Claims.Private.ProviderName, result.Principal.GetClaim(Claims.Private.ProviderName));

    var properties = new AuthenticationProperties(result.Properties.Items)
    {
        RedirectUri = result.Properties.RedirectUri ?? "/",

        // Set the creation and expiration dates of the ticket to null to decorrelate the lifetime
        // of the resulting authentication cookie from the lifetime of the identity token returned by
        // the authorization server (if applicable). In this case, the expiration date time will be
        // automatically computed by the cookie handler using the lifetime configured in the options.
        //
        // Applications that prefer binding the lifetime of the ticket stored in the authentication cookie
        // to the identity token returned by the identity provider can remove or comment these two lines:
        IssuedUtc = null,
        ExpiresUtc = null,

        // Note: this flag controls whether the authentication cookie that will be returned to the
        // browser will be treated as a session cookie (i.e destroyed when the browser is closed)
        // or as a persistent cookie. In both cases, the lifetime of the authentication ticket is
        // always stored as protected data, preventing malicious users from trying to use an
        // authentication cookie beyond the lifetime of the authentication ticket itself.
        IsPersistent = true
    };

    // If needed, the tokens returned by the authorization server can be stored in the authentication cookie.
    // To make cookies less heavy, tokens that are not used are filtered out before creating the cookie.
    properties.StoreTokens(result.Properties.GetTokens().Where(token => token.Name is
        OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken));

    return TypedResults.SignIn(new ClaimsPrincipal(identity), properties);
}).DisableAntiforgery();

// Register the endpoint responsible for handling the end session response returned by the identity provider.
app.MapMethods("/authentication/logout-callback/{provider}", [HttpMethods.Get, HttpMethods.Post], async (string? provider, HttpContext context) =>
{
    var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

    return TypedResults.Redirect(result?.Properties?.RedirectUri ?? "/");
})
.DisableAntiforgery();

app.MapForwarder("/weather-forecast", "https://localhost:7070/", builder =>
{
    builder.AddRequestTransform(async context =>
    {
        var service = context.HttpContext.RequestServices.GetRequiredService<OpenIddictClientService>();
        var token = await context.HttpContext.GetTokenAsync(OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken)
            ?? throw new InvalidOperationException("The access token cannot be retrieved.");

        var result = await service.AuthenticateWithTokenExchangeAsync(new()
        {
            SubjectToken = token,
            SubjectTokenType = TokenTypeIdentifiers.AccessToken,
            RequestedTokenType = TokenTypeIdentifiers.AccessToken,
            Scopes = ["Weather.Read"]
        });

        context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.IssuedToken);
    });

    // Prevent application cookies from being sent to the downstream API.
    builder.RequestTransforms.Add(new RequestHeaderRemoveTransform("Cookie"));
})
.RequireAuthorization();

// Before starting the host, create the database used to store the application data.
//
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DbContext>();
    await context.Database.EnsureCreatedAsync();
}

await app.RunAsync();

static ECDsaSecurityKey GetECDsaSigningKey(ReadOnlySpan<char> key)
{
    var algorithm = ECDsa.Create();
    algorithm.ImportFromPem(key);

    return new ECDsaSecurityKey(algorithm);
}
