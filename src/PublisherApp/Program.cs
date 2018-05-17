using System;
using Microsoft.Extensions.DependencyInjection;
using PublisherApp.Messaging;

namespace PublisherApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Publisher Application");
            IServiceCollection services = new ServiceCollection();
            var serviceProvider = DependencyHelper.Register(services);

            var bootstrapper = new Bootstrapper(serviceProvider);
            bootstrapper.Start(args);
        }
    }
}