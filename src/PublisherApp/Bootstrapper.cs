using System;
using Messaging.Shared;
using Microsoft.Extensions.DependencyInjection;
using PublisherApp.Messaging;

namespace PublisherApp
{
    public class Bootstrapper
    {
        private readonly IServiceProvider serviceProvider;

        public Bootstrapper(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Start(string[] args)
        {
            var messagingManager = serviceProvider.GetService<IMessagingManager>();


            Console.WriteLine("Enter a message. 'Quit' to quit.");
            var input = "";
            while ((input = Console.ReadLine()) != "Quit")
            {
                var message = new ChatEvent
                {
                    MessageText = input,
                    SenderName = "PublisherApp",
                };
                var manager = serviceProvider.GetService<IMessagingManager>();
                manager.PublishChatEventMessage(message);
            }
        }
    }
}