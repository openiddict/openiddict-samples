using System.Globalization;
using System.Security.Cryptography;
using Dantooine.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure the context to use sqlite.
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-dantooine-server.sqlite3")}");

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need
    // to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register the Identity services.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
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

        // Enable Quartz.NET integration.
        options.UseQuartz();
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        // Enable the authorization, logout, token and userinfo endpoints.
        options.SetAuthorizationEndpointUris("connect/authorize")
               .SetEndSessionEndpointUris("connect/logout")
               .SetIntrospectionEndpointUris("connect/introspect")
               .SetTokenEndpointUris("connect/token")
               .SetUserInfoEndpointUris("connect/userinfo")
               .SetEndUserVerificationEndpointUris("connect/verify");

        // Mark the "email", "profile" and "roles" scopes as supported scopes.
        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

        // Note: this sample only uses the authorization code and refresh token
        // flows but you can enable the other flows if you need to support
        // implicit, password or client credentials.
        options.AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow();

        // Register the signing and encryption credentials.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserInfoEndpointPassthrough()
               .EnableStatusCodePagesIntegration();
    })

    // Register the OpenIddict validation components.
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseStatusCodePagesWithReExecute("~/error");
    //app.UseExceptionHandler("~/error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();
app.MapRazorPages();

// Before starting the host, create the database used to store the application data.
//
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();

    await RegisterApplicationsAsync(scope.ServiceProvider);
    await RegisterScopesAsync(scope.ServiceProvider);

    static async Task RegisterApplicationsAsync(IServiceProvider provider)
    {
        var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        // API
        if (await manager.FindByClientIdAsync("resource_server_1") == null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
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
                            MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAElrZTesJa18s6LuknPtM/Kg5veUCE
                            p6YBF03eLBkapNe+P6u5zFafjm3mL5yFV7dGaxlDEe0TtXdjSUkQATtq1g==
                            -----END PUBLIC KEY-----
                            """))
                    }
                },
                Permissions =
                {
                    Permissions.Endpoints.Introspection
                }
            };

            await manager.CreateAsync(descriptor);
        }

        // Blazor Hosted
        if (await manager.FindByClientIdAsync("blazorcodeflowpkceclient") is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = "blazorcodeflowpkceclient",
                ConsentType = ConsentTypes.Explicit,
                DisplayName = "Blazor code PKCE",
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
                            MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEuXiljSpKKFtkfE+PniYWGCtPczBH
                            bnLkag0aLFN5IJss/lKz0TIKdX09suFW+/fqdT/RF5/2PI72xZ4Q5Ty+uw==
                            -----END PUBLIC KEY-----
                            """))
                    }
                },
                PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:44348/callback/logout/local")
                },
                RedirectUris =
                {
                    new Uri("https://localhost:44348/callback/login/local")
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

            descriptor.AddScopePermissions("api1");

            await manager.CreateAsync(descriptor);
        }
    }

    static async Task RegisterScopesAsync(IServiceProvider provider)
    {
        var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

        if (await manager.FindByNameAsync("api1") is null)
        {
            await manager.CreateAsync(new OpenIddictScopeDescriptor
            {
                DisplayName = "Dantooine API access",
                DisplayNames =
                {
                    [CultureInfo.GetCultureInfo("fr-FR")] = "Accès à l'API de démo"
                },
                Name = "api1",
                Resources =
                {
                    "resource_server_1"
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
