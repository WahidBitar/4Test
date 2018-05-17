using Messaging.Shared;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using SubscriberApp.Messaging;

namespace SubscriberApp
{
    internal class DependencyHelper
    {
        public static void Register(IServiceCollection services)
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
                var channel = factory.CreateConnection();
                return channel;
            });

            services.AddSingleton<IMessagingManager, RabbitMQMessagingManager>();
            services.AddScoped<IMessageConsumer<ChatEvent>, ChatEventConsumer>();
            services.AddScoped<IMessageRetryConsumer<ChatEvent>, ChatEventRetryConsumer>();
        }
    }
}