using Microsoft.AspNetCore.Authorization;

namespace WebApp
{
    public class AccessControlRequirement : IAuthorizationRequirement
    {
        public AccessControlRequirement(bool doubleCheckActionName)
        {
            DoubleCheckActionName = doubleCheckActionName;
        }

        public bool DoubleCheckActionName { get; private set; }
    }
}