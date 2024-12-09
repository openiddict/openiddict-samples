using System.Globalization;
using System.IO;
using System.Text.Json;
using Contruum.Server.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Contruum.Server;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration) => Configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Configure the context to use an in-memory store.
            options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-contruum-server.sqlite3")}");

            // Register the entity sets needed by OpenIddict.
            options.UseOpenIddict();
        });

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.AccessDeniedPath = "/connect/signin";
                options.LoginPath = "/connect/signin";
                options.LogoutPath = "/connect/signout";
            });

        // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
        // (like pruning orphaned authorizations/tokens from the database) at regular intervals.
        services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });

        // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        // Register the OpenIddict services.
        services.AddOpenIddict()
            .AddCore(options =>
            {
                // Register the Entity Framework Core models/stores.
                options.UseEntityFrameworkCore()
                       .UseDbContext<ApplicationDbContext>();

                // Enable Quartz.NET integration.
                options.UseQuartz();
            })

            .AddServer(options =>
            {
                // Enable the authorization, token, introspection and userinfo endpoints.
                options.SetAuthorizationEndpointUris(Configuration["OpenIddict:Endpoints:Authorization"]!)
                       .SetTokenEndpointUris(Configuration["OpenIddict:Endpoints:Token"]!)
                       .SetIntrospectionEndpointUris(Configuration["OpenIddict:Endpoints:Introspection"]!)
                       .SetUserInfoEndpointUris(Configuration["OpenIddict:Endpoints:Userinfo"]!)
                       .SetEndSessionEndpointUris(Configuration["OpenIddict:Endpoints:Logout"]!);

                // Enable the authorization code, implicit, hybrid and the refresh token flows.
                options.AllowAuthorizationCodeFlow()
                       .AllowImplicitFlow()
                       .AllowHybridFlow()
                       .AllowRefreshTokenFlow();

                // Expose all the supported claims in the discovery document.
                options.RegisterClaims(Configuration.GetSection("OpenIddict:Claims").Get<string[]>()!);

                // Expose all the supported scopes in the discovery document.
                options.RegisterScopes(Configuration.GetSection("OpenIddict:Scopes").Get<string[]>()!);

                // Note: an ephemeral signing key is deliberately used to make the "OP-Rotation-OP-Sig"
                // test easier to run as restarting the application is enough to rotate the keys.
                options.AddEphemeralEncryptionKey()
                       .AddEphemeralSigningKey();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                //
                // Note: the pass-through mode is not enabled for the token endpoint
                // so that token requests are automatically handled by OpenIddict.
                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableAuthorizationRequestCaching()
                       .EnableEndSessionEndpointPassthrough();

                // Register the custom event handler responsible for populating userinfo responses.
                options.AddEventHandler<HandleUserInfoRequestContext>(options => options.UseInlineHandler(context =>
                {
                    if (context.Principal.HasScope(Scopes.Profile))
                    {
                        context.GivenName = context.Principal.GetClaim(Claims.GivenName);
                        context.FamilyName = context.Principal.GetClaim(Claims.FamilyName);
                        context.BirthDate = context.Principal.GetClaim(Claims.Birthdate);
                        context.Profile = context.Principal.GetClaim(Claims.Profile);
                        context.PreferredUsername = context.Principal.GetClaim(Claims.PreferredUsername);
                        context.Website = context.Principal.GetClaim(Claims.Website);

                        context.Claims[Claims.Name] = context.Principal.GetClaim(Claims.Name);
                        context.Claims[Claims.Gender] = context.Principal.GetClaim(Claims.Gender);
                        context.Claims[Claims.MiddleName] = context.Principal.GetClaim(Claims.MiddleName);
                        context.Claims[Claims.Nickname] = context.Principal.GetClaim(Claims.Nickname);
                        context.Claims[Claims.Picture] = context.Principal.GetClaim(Claims.Picture);
                        context.Claims[Claims.Locale] = context.Principal.GetClaim(Claims.Locale);
                        context.Claims[Claims.Zoneinfo] = context.Principal.GetClaim(Claims.Zoneinfo);
                        context.Claims[Claims.UpdatedAt] = long.Parse(
                            context.Principal.GetClaim(Claims.UpdatedAt)!,
                            NumberStyles.Number, CultureInfo.InvariantCulture);
                    }

                    if (context.Principal.HasScope(Scopes.Email))
                    {
                        context.Email = context.Principal.GetClaim(Claims.Email);
                        context.EmailVerified = false;
                    }

                    if (context.Principal.HasScope(Scopes.Phone))
                    {
                        context.PhoneNumber = context.Principal.GetClaim(Claims.PhoneNumber);
                        context.PhoneNumberVerified = false;
                    }

                    if (context.Principal.HasScope(Scopes.Address))
                    {
                        context.Address = JsonSerializer.Deserialize<JsonElement>(context.Principal.GetClaim(Claims.Address)!);
                    }

                    return default;
                }));
            })

            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();

                // Enable authorization entry validation, which is required to be able
                // to reject access tokens retrieved from a revoked authorization code.
                options.EnableAuthorizationEntryValidation();
            });

        // Register the worker responsible for creating and seeding the SQL database.
        // Note: in a real world application, this step should be part of a setup script.
        services.AddHostedService<Worker>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapRazorPages());
    }
}
