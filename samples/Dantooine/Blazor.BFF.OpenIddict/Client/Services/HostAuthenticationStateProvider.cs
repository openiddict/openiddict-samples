using Blazor.BFF.OpenIddict.Shared.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blazor.BFF.OpenIddict.Client.Services
{
    // orig src https://github.com/berhir/BlazorWebAssemblyCookieAuth
    public class HostAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly TimeSpan _userCacheRefreshInterval = TimeSpan.FromSeconds(60);

        private const string LogInPath = "api/Account/Login";
        private const string LogOutPath = "api/Account/Logout";

        private readonly NavigationManager _navigation;
        private readonly HttpClient _client;
        private readonly ILogger<HostAuthenticationStateProvider> _logger;

        private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0);
        private ClaimsPrincipal _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());

        public HostAuthenticationStateProvider(NavigationManager navigation, HttpClient client, ILogger<HostAuthenticationStateProvider> logger)
        {
            _navigation = navigation;
            _client = client;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return new AuthenticationState(await GetUser(useCache: true));
        }

        public void SignIn(string customReturnUrl = null)
        {
            var returnUrl = customReturnUrl != null ? _navigation.ToAbsoluteUri(customReturnUrl).ToString() : null;
            var encodedReturnUrl = Uri.EscapeDataString(returnUrl ?? _navigation.Uri);
            var logInUrl = _navigation.ToAbsoluteUri($"{LogInPath}?returnUrl={encodedReturnUrl}");
            _navigation.NavigateTo(logInUrl.ToString(), true);
        }

        private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = false)
        {
            var now = DateTimeOffset.Now;
            if (useCache && now < _userLastCheck + _userCacheRefreshInterval)
            {
                _logger.LogDebug("Taking user from cache");
                return _cachedUser;
            }

            _logger.LogDebug("Fetching user");
            _cachedUser = await FetchUser();
            _userLastCheck = now;

            return _cachedUser;
        }

        private async Task<ClaimsPrincipal> FetchUser()
        {
            UserInfo user = null;

            try
            {
                _logger.LogInformation(_client.BaseAddress.ToString());
                user = await _client.GetFromJsonAsync<UserInfo>("api/User");
            }
            catch (Exception exc)
            {
                _logger.LogWarning(exc, "Fetching user failed.");
            }

            if (user == null || !user.IsAuthenticated)
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var identity = new ClaimsIdentity(
                nameof(HostAuthenticationStateProvider),
                user.NameClaimType,
                user.RoleClaimType);

            if (user.Claims != null)
            {
                foreach (var claim in user.Claims)
                {
                    identity.AddClaim(new Claim(claim.Type, claim.Value));
                }
            }

            return new ClaimsPrincipal(identity);
        }
    }
}
