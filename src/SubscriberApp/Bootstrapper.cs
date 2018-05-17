using System;
using Microsoft.Extensions.DependencyInjection;
using SubscriberApp.Messaging;

namespace SubscriberApp
{
    public class Bootstrapper
    {
        private readonly IServiceProvider serviceProvider;

        public Bootstrapper(IServiceCollection services)
        {
            this.serviceProvider = services.BuildServiceProvider();
        }

        public void Start(string[] args)
        {
            var messagingManager = serviceProvider.GetService<IMessagingManager>();
            messagingManager.ListenForChatMessageEvent();
            messagingManager.ListenForChatMessageRetryEvent();
        }
    }
}