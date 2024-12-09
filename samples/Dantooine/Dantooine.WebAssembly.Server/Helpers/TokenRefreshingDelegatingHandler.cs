using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Client;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.AspNetCore.OpenIddictClientAspNetCoreConstants;
using static OpenIddict.Client.OpenIddictClientModels;

namespace Dantooine.WebAssembly.Server.Helpers
{
    internal sealed class TokenRefreshingDelegatingHandler(
        OpenIddictClientService service, HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
    {
        private readonly OpenIddictClientService _service = service ?? throw new ArgumentNullException(nameof(service));

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // If an access token expiration date was returned by the authorization server and stored
            // in the authentication cookie, use it to determine whether the token is about to expire.
            // If it's not, try to use it: if the resource server returns a 401 error response, try
            // to refresh the tokens before replaying the request with the new access token attached.
            var date = GetBackchannelAccessTokenExpirationDate(request.Options);
            if (date is null || DateTimeOffset.UtcNow <= date?.AddMinutes(-5))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(Schemes.Bearer, GetBackchannelAccessToken(request.Options));

                var response = await base.SendAsync(request, cancellationToken);
                if (response.StatusCode is not HttpStatusCode.Unauthorized)
                {
                    return response;
                }

                // Note: this handler can be called concurrently for the same user if multiple HTTP
                // requests are processed in parallel: while this results in multiple refresh token
                // requests being sent concurrently, this is something OpenIddict allows during a short
                // period of time (called refresh token reuse leeway and set to 30 seconds by default).
                var result = await _service.AuthenticateWithRefreshTokenAsync(new RefreshTokenAuthenticationRequest
                {
                    CancellationToken = cancellationToken,
                    DisableUserInfo = true,
                    RefreshToken = GetRefreshToken(request.Options)
                });

                request.Headers.Authorization = new AuthenticationHeaderValue(Schemes.Bearer, result.AccessToken);

                return new TokenRefreshingHttpResponseMessage(result, await base.SendAsync(request, cancellationToken));
            }

            // Otherwise, don't bother using the existing access token and refresh tokens immediately.
            else
            {
                var result = await _service.AuthenticateWithRefreshTokenAsync(new RefreshTokenAuthenticationRequest
                {
                    CancellationToken = cancellationToken,
                    DisableUserInfo = true,
                    RefreshToken = GetRefreshToken(request.Options)
                });

                request.Headers.Authorization = new AuthenticationHeaderValue(Schemes.Bearer, result.AccessToken);

                return new TokenRefreshingHttpResponseMessage(result, await base.SendAsync(request, cancellationToken));
            }

            static string GetBackchannelAccessToken(HttpRequestOptions options) =>
                options.TryGetValue(new(Tokens.BackchannelAccessToken), out string token) ? token :
                throw new InvalidOperationException("The access token couldn't be found in the request options.");

            static DateTimeOffset? GetBackchannelAccessTokenExpirationDate(HttpRequestOptions options) =>
                options.TryGetValue(new(Tokens.BackchannelAccessTokenExpirationDate), out string token) &&
                DateTimeOffset.TryParse(token, CultureInfo.InvariantCulture, out DateTimeOffset date) ? date : null;

            static string GetRefreshToken(HttpRequestOptions options) =>
                options.TryGetValue(new(Tokens.RefreshToken), out string token) ? token :
                throw new InvalidOperationException("The refresh token couldn't be found in the request options.");
        }
    }
}
