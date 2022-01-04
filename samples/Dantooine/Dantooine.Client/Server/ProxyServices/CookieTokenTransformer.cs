using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Forwarder;

namespace Dantooine.BFF.Server
{
    internal class CookieTokenTransformer : HttpTransformer
    {
        public override async ValueTask TransformRequestAsync(HttpContext httpContext,
            HttpRequestMessage proxyRequest, string destinationPrefix)
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");
            // Copy headers normally and then remove the original host.
            // Use the destination host from proxyRequest.RequestUri instead.
            await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix);
            proxyRequest.Headers.Host = null;
            proxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

}
