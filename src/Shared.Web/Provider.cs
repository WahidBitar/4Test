using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Web
{
    public class Provider<T> : IProvider<T>
    {
        readonly IHttpContextAccessor contextAccessor;

        public Provider(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        T IProvider<T>.Get()
        {
            return contextAccessor.HttpContext.RequestServices.GetService<T>();
        }
    }

}
