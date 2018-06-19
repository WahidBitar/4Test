using System;
using Messaging.Shared;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using SubscriberApp.Messaging;

namespace SubscriberApp
{
    internal class DependencyHelper
    {
        public static IServiceProvider Register(IServiceCollection services)
        {
            services.AddSingleton(mb =>
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
            services.AddScoped<IMessageConsumer<ChatEvent>, ChatEventConsumer>();
            services.AddScoped<IMessageRetryConsumer<ChatEvent>, ChatEventRetryConsumer>();

            var serviceProvider = services.BuildServiceProvider();
            services.AddSingleton(x => serviceProvider);
            return serviceProvider;
        }
    }
}