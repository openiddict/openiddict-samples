using System.Net.Http.Headers;
using Geonosis.Ui;
using Geonosis.Ui.Client;
using Geonosis.Ui.Client.Weather;
using Geonosis.Ui.Components;
using Geonosis.Ui.Weather;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using Yarp.ReverseProxy.Transforms;
using static OpenIddict.Abstractions.OpenIddictConstants;

var issuerUrl = "https://localhost:7094";
var apiUrl = "https://localhost:7070";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add OpenIddict services
builder.Services.AddOpenIddict()
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

        options.DisableTokenStorage();

        // Add a client registration matching the client application definition in the server project.
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri(issuerUrl, UriKind.Absolute),

            ClientId = "geonosis-ui",
            ClientSecret = "super-secret-client-secret",
            // OfflineAccess is required to get refresh tokens
            Scopes = { Scopes.OfflineAccess, Scopes.Email, Scopes.Profile, Scopes.Roles },

            // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
            // URI per provider, unless all the registered providers support returning a special "iss"
            // parameter containing their URL as part of authorization responses. For more information,
            // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
            RedirectUri = new Uri("authentication/login-callback/local", UriKind.Relative),
            PostLogoutRedirectUri = new Uri("authentication/logout-callback/local", UriKind.Relative)
        });
    });

// Register the authentication and authorization services.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = Constants.LoginPath;
        options.LogoutPath = Constants.LogoutPath;
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    // Add authentication state serialization with all claims included
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

// Add authentication state provider
builder.Services.AddCascadingAuthenticationState();

// Add HttpClient for weather forecaster with base address of the weather API
builder.Services.AddHttpForwarderWithServiceDiscovery();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IWeatherForecaster, ServerWeatherForecaster>(httpClient =>
{
    httpClient.BaseAddress = new(apiUrl);
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

// Map authentication endpoints used by the authentication state provider 
//  - /authentication/login/local               for handling the login and redirection to the Auth project for authentication
//  - /authentication/logout/local              for handling the logout and redirection to the Auth project for logout
//  - /authentication/login-callback/local      for handling the authentication response from the Auth project
//  - /authentication/logout-callback/local     for handling the post-logout redirection from the Auth project
//
// Callback endpoint for handling authentication responses from Auth project
//  musth be registered in the OpenID ClientRegistration and
//  must be configured in the AddRegistration method above.
app.MapAuthenticationEndpoints();

// Map a reverse proxy endpoint for the weather API, which will forward requests to the Weather API project and add the access
// token in the authorization header.
// This is used by the client-side weather forecaster to retrieve weather forecasts from the Weather API project without
// having to worry about authentication and token management.
app.MapForwarder("/weather-forecast", apiUrl, transformBuilder =>
    {
        transformBuilder.AddRequestTransform(async transformContext =>
        {
            var openIddictClientService = transformContext.HttpContext.RequestServices.GetRequiredService<OpenIddictClientService>();
            var accessToken = await transformContext.HttpContext.GetTokenAsync(OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken)
                ?? throw new InvalidOperationException("The access token cannot be retrieved.");

            var exchangeResult = await openIddictClientService.AuthenticateWithTokenExchangeAsync(new()
            {
                SubjectToken = accessToken,
                SubjectTokenType = TokenTypeIdentifiers.AccessToken,
                RequestedTokenType = TokenTypeIdentifiers.AccessToken,
                Scopes = ["Weather.Read"],
            });

            //var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
            transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", exchangeResult.IssuedToken);
        });

        // Remove application cookies
        transformBuilder.RequestTransforms.Add(new RequestHeaderRemoveTransform("Cookie"));
    })
    .RequireAuthorization();

app.Run();
