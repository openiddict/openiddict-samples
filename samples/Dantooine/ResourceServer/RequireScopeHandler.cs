using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceServer
{
    public class RequireScopeHandler : AuthorizationHandler<RequireScope>
    {

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            RequireScope requirement
        ){
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (requirement == null)
                throw new ArgumentNullException(nameof(requirement));

            var scopeClaim =  context.User.Claims.FirstOrDefault(t => t.Type == "scope");


            if (scopeClaim != null && (context.User.HasScope("dataEventRecords")))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}