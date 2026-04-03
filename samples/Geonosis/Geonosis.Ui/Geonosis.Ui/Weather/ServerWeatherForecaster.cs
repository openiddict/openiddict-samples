using Geonosis.Ui.Client.Weather;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Geonosis.Ui.Weather;

internal sealed class ServerWeatherForecaster(
    IHttpContextAccessor accessor, HttpClient client, OpenIddictClientService service) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        var token = await accessor.HttpContext!.GetTokenAsync(OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken)
            ?? throw new InvalidOperationException("The access token cannot be retrieved.");

        var result = await service.AuthenticateWithTokenExchangeAsync(new()
        {
            SubjectToken = token,
            SubjectTokenType = TokenTypeIdentifiers.AccessToken,
            RequestedTokenType = TokenTypeIdentifiers.AccessToken,
            Scopes = ["Weather.Read"]
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, "/weather-forecast");
        request.Headers.Authorization = new("Bearer", result.IssuedToken);

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ?? [];
    }
}
