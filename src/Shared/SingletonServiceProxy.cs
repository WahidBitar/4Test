using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Shared
{
    public class SingletonServiceProxy : IServiceProxy
    {
        private readonly IHttpContextAccessor contextAccessor;

        public SingletonServiceProxy(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public IProvider<T> GetProvider<T>()
        {
            return contextAccessor.HttpContext.RequestServices.GetService<IProvider<T>>();
        }

        public T GetService<T>()
        {
            return contextAccessor.HttpContext.RequestServices.GetService<T>();
        }
    }
}