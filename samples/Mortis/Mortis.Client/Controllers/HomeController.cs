using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Security.Cookies;
using static OpenIddict.Client.Owin.OpenIddictClientOwinConstants;

namespace Mortis.Client.Controllers;

public class HomeController([FromKeyedServices("ApiClient")] HttpClient client) : Controller
{
    [HttpGet, Route("~/")]
    public ActionResult Index() => View();

    [Authorize, HttpPost, Route("~/")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Index(CancellationToken cancellationToken)
    {
        var context = HttpContext.GetOwinContext();

        var result = await context.Authentication.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationType);
        var token = result.Properties.Dictionary[Tokens.BackchannelAccessToken];

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/message");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return View(model: await response.Content.ReadAsStringAsync());
    }
}
