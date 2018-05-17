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
            Console.WriteLine("Subscriber Application");
            IServiceCollection services = new ServiceCollection();
            var serviceProvider = DependencyHelper.Register(services);

            var bootstrapper = new Bootstrapper(serviceProvider);
            bootstrapper.Start(args);
        }
    }
}