using System.Globalization;
using System.Security.Cryptography;
using Geonosis.Auth.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure the Entity Framework Core context to use SQLite.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure the context to use sqlite.
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-geonosis-auth.sqlite3")}");

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need
    // to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

// Enable the database developer exception page in development mode.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

// Register the Identity services, use the default UI and configure it to use the Entity Framework stores.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Register the OpenIddict services.

builder.Services.AddOpenIddict()
    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the Entity Framework Core stores and models.
        // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        // Enable the authorization, logout, token and userinfo endpoints.
        options.SetAuthorizationEndpointUris("connect/authorize")
               .SetEndSessionEndpointUris("connect/logout")
               .SetTokenEndpointUris("connect/token");

        // Mark the "email", "profile" and "roles" scopes as supported scopes.
        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

        // Note: this sample uses the code and refresh token flows but you can enable
        // the other flows if you need to support implicit or password flows.
        options.AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow()
               .AllowTokenExchangeFlow();

        // Register the signing and encryption credentials.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough()
               .EnableTokenEndpointPassthrough();

        options.DisableAccessTokenEncryption();
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

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
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

    await SeedClientsAsync(scope.ServiceProvider);
    await SeedRolesAsync(scope.ServiceProvider);
    await SeedSampleUsersAsync(scope.ServiceProvider);
    await SeedScopesAsync(scope.ServiceProvider);

    static async Task SeedSampleUsersAsync(IServiceProvider provider)
    {
        var manager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        // Create an admin user if it doesn't exist.
        //
        // Note: in a production application, you would likely want to have
        // a more robust seeding strategy and not hard-code credentials.
        if (await manager.FindByNameAsync("admin") is null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin@contoso.com",
                NormalizedUserName = "ADMIN@CONTOSO.COM",
                Email = "admin@contoso.COM",
                NormalizedEmail = "ADMIN@CONTOSO.COM",
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                TwoFactorEnabled = false,
                AccessFailedCount = 0,
            };

            await manager.CreateAsync(user, "Admin@12345");
            await manager.AddToRoleAsync(user, "Admin");
        }

        if (await manager.FindByNameAsync("user") is null)
        {
            var user = new ApplicationUser
            {
                UserName = "user@contoso.com",
                NormalizedUserName = "USER@CONTOSO.COM",
                Email = "user@contoso.COM",
                NormalizedEmail = "USER@CONTOSO.COM",
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                TwoFactorEnabled = false,
                AccessFailedCount = 0,
            };

            await manager.CreateAsync(user, "User@12345");
        }
    }

    static async Task SeedRolesAsync(IServiceProvider provider)
    {
        // Create an admin role if it doesn't exist.
        var manager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await manager.RoleExistsAsync("Admin"))
        {
            await manager.CreateAsync(new IdentityRole("Admin"));
        }
    }

    static async Task SeedClientsAsync(IServiceProvider provider)
    {
        var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        // Create the client application representing the UI if it doesn't exist.
        if (await manager.FindByClientIdAsync("geonosis-ui") is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = "geonosis-ui",
                ClientType = ClientTypes.Confidential,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = "Monarch UI Application",
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
                            MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEFXmvZRv1zOogKS8JP/qlGxNC+Ghr
                            UpYIGykTeHPrvrY3HFpHnQ7hvNQzLULWxLkuzsu95cMzJIuITdr7e1i8cg==
                            -----END PUBLIC KEY-----
                            """))
                    }
                },
                RedirectUris =
                {
                    new Uri("http://localhost:5027/authentication/login-callback/local"),
                    new Uri("https://localhost:7073/authentication/login-callback/local")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("http://localhost:5027/authentication/logout-callback/local"),
                    new Uri("https://localhost:7073/authentication/logout-callback/local")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.EndSession,
                    Permissions.Endpoints.Token,

                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.GrantTypes.TokenExchange,

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

            descriptor.AddScopePermissions("Weather.Read");

            await manager.CreateAsync(descriptor);
        }
    }

    static async Task SeedScopesAsync(IServiceProvider provider)
    {
        var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

        if (await manager.FindByNameAsync("Weather.Read") is null)
        {
            await manager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "Weather.Read",
                DisplayName = "Weather Read API Scope",
                DisplayNames =
                {
                    [CultureInfo.GetCultureInfo("en-US")] = "Weather Read API Scope"
                },
                Description = "Scope for reading weather data from the API",
                Resources =
                {
                    "geonosis-api"
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
