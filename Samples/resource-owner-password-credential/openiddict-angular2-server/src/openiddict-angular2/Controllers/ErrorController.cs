using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using NgOidc.Models;
namespace NgOidc.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet, HttpPost, Route("~/error")]
        public IActionResult Error()
        {
            // If the error was not caused by an invalid
            // OIDC request, display a generic error page.
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response == null)
            {
                return View();
            }
            return View(new ErrorViewModel
            {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }
    }
}
