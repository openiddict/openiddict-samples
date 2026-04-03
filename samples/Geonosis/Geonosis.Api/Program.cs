using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register the OpenIddict validation components.
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Set the authority and the audience to validate the tokens.
        options.SetIssuer("https://localhost:7094/");
        options.AddAudiences("geonosis-api");

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
  .AddPolicy("Weather.Read", policy => policy
    .RequireAuthenticatedUser()
    .RequireAssertion(context => context.User.HasScope("Weather.Read")));

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
.RequireAuthorization("Weather.Read");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
}