/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using Microsoft.AspNetCore.Mvc;
using AuthorizationServer.ViewModels.Shared;
using AspNet.Security.OpenIdConnect.Primitives;

namespace AuthorizationServer {
    public class ErrorController : Controller {
        [HttpGet, HttpPost, Route("~/error")]
        public IActionResult Error(OpenIdConnectResponse response) {
            // If the error was not caused by an invalid
            // OIDC request, display a generic error page.
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