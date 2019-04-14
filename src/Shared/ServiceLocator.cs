using System;

namespace Shared
{
    public class ServiceLocator
    {
        private static IServiceProxy singleton;

        public static T GetService<T>()
        {
            return singleton.GetService<T>();
            //return singleton.GetProvider<T>().Get();
        }

        public static void Initialize(IServiceProxy singletonService)
        {
            singleton = singletonService;
        }
    }
}
