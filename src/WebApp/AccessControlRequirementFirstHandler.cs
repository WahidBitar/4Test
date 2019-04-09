using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebApp
{    
    public class AccessControlRequirementFirstHandler : AuthorizationHandler<AccessControlRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessControlRequirement requirement)
        {
            //if (context.User.FindFirst("employeeId") != null)
            context.Fail();
            //context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}