using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OpenIddict.Validation.AspNetCore;
using Gonda.Server.Spec;

namespace Gonda.Server.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<SayReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new SayReply
            {
                Message = "Hello " + request.Name
            });
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public override Task<SayReply> SaySecret(SecretRequest request, ServerCallContext context)
        {
            var identity = context.GetHttpContext().User.Identity as ClaimsIdentity;
            
            return Task.FromResult(new SayReply
            {
                Message = $"Got it, {identity?.Name}... \"{request.Secret}\" is a secret!"
            });
        }
    }
}