using System.Configuration;
using System.Globalization;
using Dantooine.WebAssembly.Server;
using Dantooine.WebAssembly.Server.Helpers;
using Dantooine.WebAssembly.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using Quartz;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Abstractions.OpenIddictExceptions;
using static OpenIddict.Client.AspNetCore.OpenIddictClientAspNetCoreConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure the context to use sqlite.
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-dantooine-webassembly-server.sqlite3")}");

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need
    // to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

// Configure the antiforgery stack to allow extracting
// antiforgery tokens from the X-XSRF-TOKEN header.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "__Host-X-XSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})

.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations from the database) at regular intervals.
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

        // Developers who prefer using MongoDB can remove the previous lines
        // and configure OpenIddict to use the specified MongoDB database:
        // options.UseMongoDb()
        //        .UseDatabase(new MongoClient().GetDatabase("openiddict"));

        // Enable Quartz.NET integration.
        options.UseQuartz();
    })

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Note: this sample uses the authorization code and refresh token
        // flows, but you can enable the other flows if necessary.
        options.AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow();

        // Register the signing and encryption credentials used to protect
        // sensitive data like the state tokens produced by OpenIddict.
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
            Issuer = new Uri("https://localhost:44319/", UriKind.Absolute),

            ClientId = "blazorcodeflowpkceclient",
            ClientSecret = "codeflow_pkce_client_secret",
            Scopes = { Scopes.OfflineAccess, Scopes.Profile, "api1" },

            // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
            // URI per provider, unless all the registered providers support returning a special "iss"
            // parameter containing their URL as part of authorization responses. For more information,
            // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
            RedirectUri = new Uri("callback/login/local", UriKind.Relative),
            PostLogoutRedirectUri = new Uri("callback/logout/local", UriKind.Relative)
        });
    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Create an authorization policy used by YARP when forwarding requests
// from the WASM application to the Dantooine.Api resource server.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CookieAuthenticationPolicy", builder =>
    {
        builder.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
        builder.RequireAuthenticatedUser();
    });

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builder =>
    {
        builder.AddRequestTransform(async context =>
        {
            // Attach the access token, access token expiration date and refresh token resolved from the authentication
            // cookie to the request options so they can later be resolved from the delegating handler and attached
            // to the request message or used to refresh the tokens if the server returned a 401 error response.
            //
            // Alternatively, the user tokens could be stored in a database or a distributed cache.

            var result = await context.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result is not { Succeeded: true })
            {
                return;
            }

            context.ProxyRequest.Options.Set(
                key: new(Tokens.BackchannelAccessToken),
                value: result.Properties.GetTokenValue(Tokens.BackchannelAccessToken));

            context.ProxyRequest.Options.Set(
                key: new(Tokens.BackchannelAccessTokenExpirationDate),
                value: result.Properties.GetTokenValue(Tokens.BackchannelAccessTokenExpirationDate));

            context.ProxyRequest.Options.Set(
                key: new(Tokens.RefreshToken),
                value: result.Properties.GetTokenValue(Tokens.RefreshToken));
        });

        builder.AddResponseTransform(async context =>
        {
            // If tokens were refreshed during the request handling (e.g due to the stored access token being
            // expired or a 401 error response being returned by the resource server), extract and attach them
            // to the authentication cookie that will be returned to the browser: doing that is essential as
            // OpenIddict uses rolling refresh tokens: if the refresh token wasn't replaced, future refresh
            // token requests would end up being rejected as they would be treated as replayed requests.

            if (context.ProxyResponse is not TokenRefreshingHttpResponseMessage response)
            {
                return;
            }

            var result = await context.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result is not { Succeeded: true })
            {
                return;
            }

            // Override the tokens using the values returned in the token response.
            var properties = result.Properties.Clone();
            properties.UpdateTokenValue(Tokens.BackchannelAccessToken, response.RefreshTokenAuthenticationResult.AccessToken);

            properties.UpdateTokenValue(Tokens.BackchannelAccessTokenExpirationDate,
                response.RefreshTokenAuthenticationResult.AccessTokenExpirationDate?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);

            // Note: if no refresh token was returned, preserve the refresh token initially returned.
            if (!string.IsNullOrEmpty(response.RefreshTokenAuthenticationResult.RefreshToken))
            {
                properties.UpdateTokenValue(Tokens.RefreshToken, response.RefreshTokenAuthenticationResult.RefreshToken);
            }

            // Remove the redirect URI from the authentication properties
            // to prevent the cookies handler from genering a 302 response.
            properties.RedirectUri = null;

            // Replace the creation/expiration dates of the authentication ticket to extend the lifetime of the cookie.
            //
            // Note: doing that is not mandatory: if the expiration date is not replaced here, the resulting cookie
            // will have the same expiration date as the authentication cookie present in the HTTP request headers.
            //
            // In any case, if the sliding expiration mechanism is enabled, the cookie (but not the data it contains)
            // will be automatically renewed by the cookie handler upon reaching half of the cookie's lifespan.
            properties.IssuedUtc = TimeProvider.System.GetUtcNow();
            properties.ExpiresUtc = properties.IssuedUtc + TimeSpan.FromDays(7);

            // Note: this event handler can be called concurrently for the same user if multiple HTTP
            // responses are returned in parallel: in this case, the browser will always store the latest
            // cookie received and the refresh tokens stored in the other cookies will be discarded.
            await context.HttpContext.SignInAsync(result.Ticket.AuthenticationScheme, result.Principal, properties);
        });
    });

// Replace the default HTTP client factory used by YARP by an instance able to inject the HTTP delegating
// handler that will be used to attach the access tokens to HTTP requests or refresh tokens if necessary.
builder.Services.Replace(ServiceDescriptor.Singleton<IForwarderHttpClientFactory, TokenRefreshingForwarderHttpClientFactory>());

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Note: by default, the cookie authentication middleware automatically redirects
// the user agent to the login page configured in the cookie authentication options.
// In this case, this behavior is not desirable as an HTTP 401 response MUST be
// returned to the WASM client to automatically redirect the user agent to the
// login page. As such, this logic is disabled for all proxied requests.
app.MapReverseProxy(ConfigureProxyPipeline).DisableCookieRedirect();

app.MapFallbackToPage("/_Host");

// Before starting the host, create the database used to store the application data.
//
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

await app.RunAsync();

static void ConfigureProxyPipeline(IReverseProxyApplicationBuilder app)
{
    app.Use(async (context, next) =>
    {
        await next();

        // Note: if an "access_denied" error occurred while trying to refresh tokens,
        // trigger a cookie authentication challenge to let the WASM client know that
        // the user should be redirected to the login page to re-authenticate.
        var exception = context.GetForwarderErrorFeature()?.Exception;
        if (exception is ProtocolException { Error: Errors.InvalidGrant })
        {
            context.Response.Clear();

            await context.ChallengeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    });
}