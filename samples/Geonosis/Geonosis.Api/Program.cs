using System.Security.Claims;
using OpenIddict.Validation;
using OpenIddict.Validation.AspNetCore;

var issuerUrl = "https://localhost:7094";
var weatherReadAuthPolicy = "Weather.Read";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register the OpenIddict validation components.
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Note: the validation handler uses OpenID Connect discovery
        // to retrieve the address of the introspection endpoint.
        options.SetIssuer(issuerUrl);
        options.AddAudiences("geonosis-api");

        // Split the "scope" claim into multiple claims if it contains multiple values separated by spaces.
        options.AddEventHandler<OpenIddictValidationEvents.ValidateTokenContext>(builder =>
        {
            builder.UseInlineHandler(context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    var scopeClaim = identity.FindAll("scope");
                    foreach (var claim in scopeClaim)
                    {
                        if (claim != null && claim.Value.Contains(' '))
                        {
                            // Remove the original "scope" claim
                            identity.RemoveClaim(claim);

                            // Add a "scope" claim for each value
                            foreach (var scope in claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                            {
                                identity.AddClaim(new Claim("scope", scope, ClaimValueTypes.String, claim.Issuer, claim.OriginalIssuer));
                            }
                        }
                    }

                }

                return default;
            });
        });

        // Register the System.Net.Http integration.
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

// Add a policy that requires the "Weather.Read" scope.
builder.Services.AddAuthorizationBuilder()
  .AddPolicy(weatherReadAuthPolicy, policy => policy
    .RequireAuthenticatedUser()
    .RequireClaim("scope", "Weather.Read"));


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

// A sample endpoint that requires the "Weather.Read" scope to be accessed.
app.MapGet("/weather-forecast", () =>
    {
        string[] summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();

        return forecast;
    })
    .RequireAuthorization(weatherReadAuthPolicy);

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
}