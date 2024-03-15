using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
        options.UseIntrospection()
               .SetClientId("resource_server_1")
               .SetClientSecret("846B62D0-DEF9-4215-A99D-86E6B8DAB342");

        // Register the System.Net.Http integration.
        options.UseSystemNetHttp();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

builder.Services.AddCors();
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:5112"));
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("api", [Authorize] (ClaimsPrincipal user) => $"{user.Identity!.Name} is allowed to access Api1.");

app.Run();
