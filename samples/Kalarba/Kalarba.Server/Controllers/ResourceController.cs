using System.Net;
using System.Security.Claims;
using System.Web.Http;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Kalarba.Server.Controllers
{
    [RoutePrefix("api")]
    public class ResourceController : ApiController
    {
        [Authorize, HttpGet]
        [Route("message")]
        public IHttpActionResult GetMessage()
        {
            var principal = (ClaimsPrincipal) User;
            var name = principal.FindFirst(Claims.Name).Value;

            return Content(HttpStatusCode.OK, $"{name} has been successfully authenticated.");
        }
    }
}