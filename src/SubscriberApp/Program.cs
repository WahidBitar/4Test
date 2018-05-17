using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace SubscriberApp
{
    class Program
    {
        static void Main(string[] args)
        {    
            IServiceCollection services = new ServiceCollection();
            DependencyHelper.Register(services);

            var bootstrapper = new Bootstrapper(services);
            bootstrapper.Start(args);
        }
    }
}