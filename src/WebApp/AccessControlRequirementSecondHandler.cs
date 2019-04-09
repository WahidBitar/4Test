using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebApp
{
    public class AccessControlRequirementSecondHandler : AuthorizationHandler<AccessControlRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessControlRequirement requirement)
        {
            if (context.User.FindFirst("first_name").ToString() == "أحمد")
                context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}