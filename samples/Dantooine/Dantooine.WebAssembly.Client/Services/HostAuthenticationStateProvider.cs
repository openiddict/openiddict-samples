using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Dantooine.WebAssembly.Client.Services;

public class HostAuthenticationStateProvider(HttpClient client) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Note: the resulting authentication state is deliberately not cached, to ensure that the logged-in
        // user information is refreshed when navigating to a different page. While a caching strategy can be
        // implemented, doing so must be done with care to ensure that the cached authentication state is
        // invalidated when the user logs out, and that the cache is refreshed when the user logs in again.

        string name;

        try
        {
            name = await client.GetStringAsync("api/current-user-name");
        }

        catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity("Cookies");
        identity.AddClaim(new Claim(ClaimTypes.Name, name));

        var state = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(state));

        return state;
    }
}
