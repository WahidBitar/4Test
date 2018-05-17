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
            Console.WriteLine("Hello World!");

            IServiceCollection services = new ServiceCollection();

            var bootstrapper = new Bootstrapper(services);
            bootstrapper.Start(args);
        }
    }
}