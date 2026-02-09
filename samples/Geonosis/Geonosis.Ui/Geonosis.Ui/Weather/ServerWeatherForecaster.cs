using System.Net;
using System.Net.Http.Headers;
using Geonosis.Ui.Client.Weather;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Geonosis.Ui.Weather
{
    internal sealed class ServerWeatherForecaster() : IWeatherForecaster
    //internal sealed class ServerWeatherForecaster(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : IWeatherForecaster
    {
        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
        {
            //// Retrieve the data stored by OpenIddict in the state token created when the logout was triggered.
            ////var result = await httpContextAccessor.HttpContext.User .AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            ////var openIddictClientService = transformContext.HttpContext.RequestServices.GetRequiredService<OpenIddictClientService>();
            ////var accessToken = await transformContext.HttpContext.GetTokenAsync(OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken)
            ////    ?? throw new InvalidOperationException("The access token cannot be retrieved.");

            //var exchangeResult = await openIddictClientService.AuthenticateWithTokenExchangeAsync(new()
            //{
            //    SubjectToken = accessToken,
            //    SubjectTokenType = TokenTypeIdentifiers.AccessToken,
            //    RequestedTokenType = TokenTypeIdentifiers.AccessToken,
            //    Scopes = ["Weather.Read"],
            //});

            //using var request = new HttpRequestMessage(HttpMethod.Get, "/weather-forecast");
            //request.Headers.Authorization = new("Bearer", exchangeResult.IssuedToken);

            //using var response = await httpClient.SendAsync(request);
            //response.EnsureSuccessStatusCode();

            //return await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ?? [];
            throw new NotImplementedException("This method is not implemented yet.");
        }
    }
}
