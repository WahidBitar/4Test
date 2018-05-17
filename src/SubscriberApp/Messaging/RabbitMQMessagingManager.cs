using System;
using System.Collections.Concurrent;
using System.Text;
using Messaging.Shared;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SubscriberApp.Messaging
{
    public class RabbitMQMessagingManager : IMessagingManager
    {
        private readonly IModel amqpChannel;
        private readonly IServiceProvider serviceProvider;
        public const string ContentType = "application/json";
        private const string chatEventQueueName = "ChatMessageEvent";
        private const string chatEventRetryQueueName = "ChatMessageEvent_Retry";
        private const string chatEventErrorQueueName = "ChatMessageEvent_Error";
        private const int maximumAllowedRetry = 3;

        public RabbitMQMessagingManager(IModel channel, IServiceProvider serviceProvider)
        {
            this.amqpChannel = channel;
            this.serviceProvider = serviceProvider;
        }

        public void ListenForChatMessageEvent()
        {
            var queue = amqpChannel.QueueDeclare(
                queue: chatEventQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var eventingConsumer = new EventingBasicConsumer(amqpChannel);
            eventingConsumer.Received += (con, eventArgs) =>
            {
                using (serviceProvider.CreateScope())
                {
                    //// one way

                    /*var payloadString = Encoding.UTF8.GetString(eventArgs.Body);
                    var message = JsonConvert.DeserializeObject<ChatEvent>(payloadString);
                    var consumer = serviceProvider.GetService<IMessageConsumer<ChatEvent>>();
                    consumer.Consume(message);*/

                    //// another way
                    var consumer = serviceProvider.GetService<IMessageConsumer<ChatEvent>>();
                    consumer.Consume(eventArgs.Body);

                    //Finally
                    amqpChannel.BasicAck(eventArgs.DeliveryTag, false);
                }
            };
            amqpChannel.BasicConsume(chatEventQueueName, false, eventingConsumer);
        }

        public void ListenForChatMessageRetryEvent()
        {
            var queue = amqpChannel.QueueDeclare(queue: chatEventRetryQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var eventingConsumer = new EventingBasicConsumer(amqpChannel);
            eventingConsumer.Received += (channel, eventArgs) =>
            {
                using (serviceProvider.CreateScope())
                {
                    if (int.TryParse(eventArgs.BasicProperties?.Headers?["RetryAttempts"]?.ToString(), out var retryAttempts))
                    {
                        var consumer = serviceProvider.GetService<IMessageRetryConsumer<ChatEvent>>();
                        consumer.Consume(eventArgs.Body, retryAttempts);
                    }

                    //Finally
                    amqpChannel.BasicAck(eventArgs.DeliveryTag, false);
                }
            };

            amqpChannel.BasicConsume(chatEventRetryQueueName, false, eventingConsumer);
        }

        public void PublishRetryChatEventMessage(ChatEvent message, int retryAttempts = 0)
        {
            var messageProperties = amqpChannel.CreateBasicProperties();
            messageProperties.ContentType = ContentType;

            messageProperties.Headers = new ConcurrentDictionary<string, object>();
            messageProperties.Headers.Add("RetryAttempts", (1 + retryAttempts));

            amqpChannel.BasicPublish("", chatEventRetryQueueName, messageProperties, serialize(message));
        }

        public void PublishErrorChatEventMessage(ChatEvent message)
        {
            var messageProperties = amqpChannel.CreateBasicProperties();
            messageProperties.ContentType = ContentType;
            amqpChannel.BasicPublish("", chatEventErrorQueueName, messageProperties, serialize(message));
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