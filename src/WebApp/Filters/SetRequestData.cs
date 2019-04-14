using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared;

namespace WebApp.Filters
{
    public class SetRequestDataFilter : ActionFilterAttribute
    {
        private readonly IRequestData requestData;

        public SetRequestDataFilter(IRequestData requestData)
        {
            this.requestData = requestData;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                requestData.Something = descriptor.MethodInfo.Name;
            }
            base.OnActionExecuting(context);
        }
    }
}
