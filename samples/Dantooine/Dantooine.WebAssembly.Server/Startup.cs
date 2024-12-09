using System;
using System.Globalization;
using System.IO;
using Dantooine.WebAssembly.Server.Helpers;
using Dantooine.WebAssembly.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OpenIddict.Client;
using Quartz;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.AspNetCore.OpenIddictClientAspNetCoreConstants;
using static OpenIddict.Client.OpenIddictClientModels;

namespace Dantooine.WebAssembly.Server;

public class Startup
{
    public Startup(IConfiguration configuration)
        => Configuration = configuration;

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
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
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
            options.Cookie.Name = "__Host-X-XSRF-TOKEN";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })

        .AddCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
            options.SlidingExpiration = false;
        });

        // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
        // (like pruning orphaned authorizations from the database) at regular intervals.
        services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });

        // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.AddOpenIddict()

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
                       .SetProductInformation(typeof(Startup).Assembly);

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

        services.AddControllersWithViews();
        services.AddRazorPages();

        // Create an authorization policy used by YARP when forwarding requests
        // from the WASM application to the Dantooine.Api resource server.
        services.AddAuthorization(options => options.AddPolicy("CookieAuthenticationPolicy", builder =>
        {
            builder.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
            builder.RequireAuthenticatedUser();
        }));

        services.AddReverseProxy()
            .LoadFromConfig(Configuration.GetSection("ReverseProxy"))
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

                    context.ProxyRequest.Options.Set(
                        key  : new(Tokens.BackchannelAccessToken),
                        value: result.Properties.GetTokenValue(Tokens.BackchannelAccessToken));

                    context.ProxyRequest.Options.Set(
                        key  : new(Tokens.BackchannelAccessTokenExpirationDate),
                        value: result.Properties.GetTokenValue(Tokens.BackchannelAccessTokenExpirationDate));

                    context.ProxyRequest.Options.Set(
                        key  : new(Tokens.RefreshToken),
                        value: result.Properties.GetTokenValue(Tokens.RefreshToken));
                });

                builder.AddResponseTransform(async context =>
                {
                    // If tokens were refreshed during the request handling (e.g due to the stored access token being
                    // expired or a 401 error response being returned by the resource server), extract and attach them
                    // to the authentication cookie that will be returned to the browser: doing that is essential as
                    // OpenIddict uses rolling refresh tokens: if the refresh token wasn't replaced, future refresh
                    // token requests would end up being rejected as they would be treated as replayed requests.

                    if (context.ProxyResponse is not TokenRefreshingHttpResponseMessage {
                        RefreshTokenAuthenticationResult: RefreshTokenAuthenticationResult } response)
                    {
                        return;
                    }

                    var result = await context.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    // Override the tokens using the values returned in the token response.
                    var properties = result.Properties.Clone();
                    properties.UpdateTokenValue(Tokens.BackchannelAccessToken, response.RefreshTokenAuthenticationResult.AccessToken);

                    properties.UpdateTokenValue(Tokens.BackchannelAccessTokenExpirationDate,
                        response.RefreshTokenAuthenticationResult.AccessTokenExpirationDate?.ToString(CultureInfo.InvariantCulture));

                    // Note: if no refresh token was returned, preserve the refresh token initially returned.
                    if (!string.IsNullOrEmpty(response.RefreshTokenAuthenticationResult.RefreshToken))
                    {
                        properties.UpdateTokenValue(Tokens.RefreshToken, response.RefreshTokenAuthenticationResult.RefreshToken);
                    }

                    // Remove the redirect URI from the authentication properties
                    // to prevent the cookies handler from genering a 302 response.
                    properties.RedirectUri = null;

                    // Note: this event handler can be called concurrently for the same user if multiple HTTP
                    // responses are returned in parallel: in this case, the browser will always store the latest
                    // cookie received and the refresh tokens stored in the other cookies will be discarded.
                    await context.HttpContext.SignInAsync(result.Ticket.AuthenticationScheme, result.Principal, properties);
                });
            });

        // Replace the default HTTP client factory used by YARP by an instance able to inject the HTTP delegating
        // handler that will be used to attach the access tokens to HTTP requests or refresh tokens if necessary.
        services.Replace(ServiceDescriptor.Singleton<IForwarderHttpClientFactory, TokenRefreshingForwarderHttpClientFactory>());

        // Register the worker responsible for creating the database used to store tokens.
        // Note: in a real world application, this step should be part of a setup script.
        services.AddHostedService<Worker>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapReverseProxy();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
