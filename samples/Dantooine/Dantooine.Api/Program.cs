using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register the OpenIddict validation components.
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Note: the validation handler uses OpenID Connect discovery
        // to retrieve the address of the introspection endpoint.
        options.SetIssuer("https://localhost:44319/");
        options.AddAudiences("resource_server_1");

        // Configure the validation handler to use introspection and register the client
        // credentials used when communicating with the remote introspection endpoint.
        //
        // Note: instead of sending a client secret, this application authenticates by
        // generating client assertions that are signed using an ECDSA signing key.
        options.UseIntrospection()
               .SetClientId("resource_server_1")
               .AddSigningKey(GetECDsaSigningKey($"""
                    -----BEGIN EC PRIVATE KEY-----
                    MHcCAQEEIFV0jPBUM8yaqQCmbgJ3IYmebIk5maW7XJCWUSZ8N2lEoAoGCCqGSM49
                    AwEHoUQDQgAElrZTesJa18s6LuknPtM/Kg5veUCEp6YBF03eLBkapNe+P6u5zFaf
                    jm3mL5yFV7dGaxlDEe0TtXdjSUkQATtq1g==
                    -----END EC PRIVATE KEY-----
                    """));

        // Register the System.Net.Http integration.
        options.UseSystemNetHttp();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("api/downstream-api", () => new[] { "data1", "data2" }).RequireAuthorization();

app.Run();

static ECDsaSecurityKey GetECDsaSigningKey(ReadOnlySpan<char> key)
{
    var algorithm = ECDsa.Create();
    algorithm.ImportFromPem(key);

    return new ECDsaSecurityKey(algorithm);
}
