using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Client.AspNetCore;

namespace Velusia.Client.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    [HttpGet("~/")]
    public ActionResult Index() => View();

    [Authorize, HttpPost("~/")]
    public async Task<ActionResult> Index(CancellationToken cancellationToken)
    {
        var token = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken);

        using var client = _httpClientFactory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44313/api/message");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return View(model: await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
