using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OpenIddict.Validation.Owin;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Mortis.Server.Controllers
{
    [HostAuthentication(OpenIddictValidationOwinDefaults.AuthenticationType)]
    public class ResourceController : ApiController
    {
        [Authorize, HttpGet, Route("~/api/message")]
        public async Task<IHttpActionResult> GetMessage()
        {
            var context = Request.GetOwinContext();

            var user = await context.GetUserManager<ApplicationUserManager>().FindByIdAsync(
                ((ClaimsPrincipal) User).FindFirst(Claims.Subject).Value);
            if (user is null)
            {
                context.Authentication.Challenge(
                    authenticationTypes: OpenIddictValidationOwinDefaults.AuthenticationType,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictValidationOwinConstants.Properties.Error] = Errors.InvalidToken,
                        [OpenIddictValidationOwinConstants.Properties.ErrorDescription] =
                            "The specified access token is bound to an account that no longer exists."
                    }));
                return Unauthorized();
            }

            return ResponseMessage(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{user.UserName} has been successfully authenticated.")
            });
        }
    }
}
