//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace Blazor.BFF.OpenIddict.Server.Controllers
//{
//    [ValidateAntiForgeryToken]
//    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
//    [ApiController]
//    [Route("api/[controller]")]
//    public class DownstreamApiController : ControllerBase
//    {
//        [HttpGet]
//        public async Task<IEnumerable<string>> GetAsync()
//        {
//            var accessToken = await HttpContext.GetTokenAsync("access_token");

//            return new List<string> { accessToken, "more data", "loads of data" };
//        }
//    }
//}
