using System;
using Microsoft.Extensions.DependencyInjection;
using PublisherApp.Messaging;
using RabbitMQ.Client;

namespace PublisherApp
{
    internal class DependencyHelper
    {
        public static IServiceProvider Register(IServiceCollection services)
        {
            services.AddSingleton<IModel>(mb =>
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    UserName = "user",
                    Password = "pass",
                    VirtualHost = "ChatTest"
                };
                var connection = factory.CreateConnection();

                return connection.CreateModel();
            });

            services.AddSingleton<IMessagingManager, RabbitMQMessagingManager>();

            return services.BuildServiceProvider();
        }
    }
}