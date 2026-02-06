using Geonosis.Ui;
using Geonosis.Ui.Client;
using Geonosis.Ui.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Client;
using static OpenIddict.Abstractions.OpenIddictConstants;

var issuerUrl = "https://localhost:7094";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add OpenIddict services
builder.Services.AddOpenIddict()
    .AddClient(options =>
    {
        // Note: this sample uses the authorization code and refresh token
        // flows, but you can enable the other flows if necessary.
        options.AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow();

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
            ClientSecret = "super-secret-client-id",
            // OfflineAccess is required to get refresh tokens
            Scopes = { Scopes.OfflineAccess, Scopes.Email, Scopes.Profile },

            // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
            // URI per provider, unless all the registered providers support returning a special "iss"
            // parameter containing their URL as part of authorization responses. For more information,
            // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
            RedirectUri = new Uri("authentication/login-callback/local", UriKind.Relative),
            PostLogoutRedirectUri = new Uri("authentication/logout-callback/local", UriKind.Relative)
        });
    })
    .AddValidation(options =>
    {
        // Note: the validation handler uses OpenID Connect discovery
        // to retrieve the address of the introspection endpoint.
        options.SetIssuer(new Uri(issuerUrl, UriKind.Absolute));

        // Register the System.Net.Http integration.
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
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
app.MapGroup("/authentication").MapAuthenticationEndpoints();

app.Run();
