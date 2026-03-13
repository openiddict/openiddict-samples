using System.Globalization;
using System.Text.Json.Nodes;
using Contruum.Server.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure the context to use an in-memory store.
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-contruum-server.sqlite3")}");

    // Register the entity sets needed by OpenIddict.
    options.UseOpenIddict();
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/connect/signin";
        options.LoginPath = "/connect/signin";
        options.LogoutPath = "/connect/signout";
    });

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
builder.Services.AddQuartz(options =>
{
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

// Register the OpenIddict services.
builder.Services.AddOpenIddict()
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
        options.SetAuthorizationEndpointUris(builder.Configuration["OpenIddict:Endpoints:Authorization"]!)
               .SetTokenEndpointUris(builder.Configuration["OpenIddict:Endpoints:Token"]!)
               .SetIntrospectionEndpointUris(builder.Configuration["OpenIddict:Endpoints:Introspection"]!)
               .SetUserInfoEndpointUris(builder.Configuration["OpenIddict:Endpoints:Userinfo"]!)
               .SetEndSessionEndpointUris(builder.Configuration["OpenIddict:Endpoints:Logout"]!);

        // Enable the authorization code, implicit, hybrid and the refresh token flows.
        options.AllowAuthorizationCodeFlow()
               .AllowImplicitFlow()
               .AllowHybridFlow()
               .AllowRefreshTokenFlow();

        // Expose all the supported claims in the discovery document.
        options.RegisterClaims(builder.Configuration.GetSection("OpenIddict:Claims").Get<string[]>()!);

        // Expose all the supported scopes in the discovery document.
        options.RegisterScopes(builder.Configuration.GetSection("OpenIddict:Scopes").Get<string[]>()!);

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
               .EnableEndSessionEndpointPassthrough();

        // Register the custom event handler responsible for populating userinfo responses.
        options.AddEventHandler<HandleUserInfoRequestContext>(options => options.UseInlineHandler(static context =>
        {
            if (context.AccessTokenPrincipal.HasScope(Scopes.Profile))
            {
                context.GivenName = context.AccessTokenPrincipal.GetClaim(Claims.GivenName);
                context.FamilyName = context.AccessTokenPrincipal.GetClaim(Claims.FamilyName);
                context.BirthDate = context.AccessTokenPrincipal.GetClaim(Claims.Birthdate);
                context.Profile = context.AccessTokenPrincipal.GetClaim(Claims.Profile);
                context.PreferredUsername = context.AccessTokenPrincipal.GetClaim(Claims.PreferredUsername);
                context.Website = context.AccessTokenPrincipal.GetClaim(Claims.Website);

                context.Claims[Claims.Name] = context.AccessTokenPrincipal.GetClaim(Claims.Name);
                context.Claims[Claims.Gender] = context.AccessTokenPrincipal.GetClaim(Claims.Gender);
                context.Claims[Claims.MiddleName] = context.AccessTokenPrincipal.GetClaim(Claims.MiddleName);
                context.Claims[Claims.Nickname] = context.AccessTokenPrincipal.GetClaim(Claims.Nickname);
                context.Claims[Claims.Picture] = context.AccessTokenPrincipal.GetClaim(Claims.Picture);
                context.Claims[Claims.Locale] = context.AccessTokenPrincipal.GetClaim(Claims.Locale);
                context.Claims[Claims.Zoneinfo] = context.AccessTokenPrincipal.GetClaim(Claims.Zoneinfo);
                context.Claims[Claims.UpdatedAt] = long.Parse(
                    context.AccessTokenPrincipal.GetClaim(Claims.UpdatedAt)!,
                    NumberStyles.Number, CultureInfo.InvariantCulture);
            }

            if (context.AccessTokenPrincipal.HasScope(Scopes.Email))
            {
                context.Email = context.AccessTokenPrincipal.GetClaim(Claims.Email);
                context.EmailVerified = false;
            }

            if (context.AccessTokenPrincipal.HasScope(Scopes.Phone))
            {
                context.PhoneNumber = context.AccessTokenPrincipal.GetClaim(Claims.PhoneNumber);
                context.PhoneNumberVerified = false;
            }

            if (context.AccessTokenPrincipal.HasScope(Scopes.Address))
            {
                context.Address = JsonNode.Parse(context.AccessTokenPrincipal.GetClaim(Claims.Address)!)!.AsObject();
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

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Before starting the host, create the database used to store the application data.
//
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();

    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    // Retrieve the client definitions from the configuration
    // and insert them in the applications table if necessary.
    var descriptors = app.Configuration.GetSection("OpenIddict:Clients").Get<OpenIddictApplicationDescriptor[]>();
    if (descriptors is not { Length: > 0 })
    {
        throw new InvalidOperationException("No client application was found in the configuration file.");
    }

    foreach (var descriptor in descriptors)
    {
        if (await manager.FindByClientIdAsync(descriptor.ClientId!) is not null)
        {
            continue;
        }

        await manager.CreateAsync(descriptor);
    }
}

await app.RunAsync();
