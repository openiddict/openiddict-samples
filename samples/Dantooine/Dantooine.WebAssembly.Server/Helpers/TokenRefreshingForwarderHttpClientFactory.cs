using System;
using System.Net.Http;
using OpenIddict.Client;
using Yarp.ReverseProxy.Forwarder;

namespace Dantooine.WebAssembly.Server.Helpers
{
    internal sealed class TokenRefreshingForwarderHttpClientFactory(OpenIddictClientService service) : ForwarderHttpClientFactory
    {
        private readonly OpenIddictClientService _service = service ?? throw new ArgumentNullException(nameof(service));

        protected override HttpMessageHandler WrapHandler(ForwarderHttpClientContext context, HttpMessageHandler handler)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(handler);

            return new TokenRefreshingDelegatingHandler(_service, handler);
        }
    }
}
