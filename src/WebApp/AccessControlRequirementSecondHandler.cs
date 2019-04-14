using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebApp
{
    public class AccessControlRequirementSecondHandler : AuthorizationHandler<AccessControlRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessControlRequirement requirement)
        {
            var x = context.User.FindFirst("first_name").Value;
            //context.Succeed(requirement);
            context.Fail();
            return Task.CompletedTask;
        }
    }
}