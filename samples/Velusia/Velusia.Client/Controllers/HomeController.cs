using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Client.AspNetCore;

namespace Velusia.Client.Controllers;

public class HomeController([FromKeyedServices("ApiClient")] HttpClient client) : Controller
{
    [HttpGet("~/")]
    public ActionResult Index() => View();

    [Authorize, HttpPost("~/")]
    public async Task<ActionResult> Index(CancellationToken cancellationToken)
    {
        // For scenarios where the default authentication handler configured in the ASP.NET Core
        // authentication options shouldn't be used, a specific scheme can be specified here.
        var token = await HttpContext.GetTokenAsync(OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/message");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return View(model: await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
