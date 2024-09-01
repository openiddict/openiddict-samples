using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Velusia.Client.ViewModels.Shared;

namespace Velusia.Client;

public class ErrorController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true), Route("~/error")]
    public IActionResult Error()
    {
        // If the error originated from the OpenIddict client, render the error details.
        var response = HttpContext.GetOpenIddictClientResponse();
        if (response is not null)
        {
            return View(new ErrorViewModel
            {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }

        return View(new ErrorViewModel());
    }
}
