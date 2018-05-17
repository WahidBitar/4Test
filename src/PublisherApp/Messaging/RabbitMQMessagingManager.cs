using System;
using System.Text;
using Messaging.Shared;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PublisherApp.Messaging
{
    public class RabbitMQMessagingManager : IMessagingManager
    {
        private readonly IModel amqpChannel;
        private readonly IServiceProvider serviceProvider;
        public const string ContentType = "application/json";
        private const string chatEventQueueName = "ChatMessageEvent";

        public RabbitMQMessagingManager(IModel channel, IServiceProvider serviceProvider)
        {
            this.amqpChannel = channel;
            this.serviceProvider = serviceProvider;
        }

        public void PublishChatEventMessage(ChatEvent message)
        {
            var messageProperties = amqpChannel.CreateBasicProperties();
            messageProperties.ContentType = ContentType;
            amqpChannel.BasicPublish("", chatEventQueueName, messageProperties, serialize(message));
        }


        private static byte[] serialize(object obj)
        {
            if (obj == null)
                return null;

            var json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}