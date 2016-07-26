/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using AuthorizationServer.ViewModels.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationServer {
    public class ErrorController : Controller {
        [HttpGet, HttpPost, Route("~/error")]
        public IActionResult Error() {
            // If the error was not caused by an invalid
            // OIDC request, display a generic error page.
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response == null) {
                return View(new ErrorViewModel());
            }

            return View(new ErrorViewModel {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }
    }
}