﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DistributeMe.ImageProcessing.Messaging;
using DistributeMe.ImageProcessing.WPF.Consumers;
using DistributeMe.ImageProcessing.WPF.Messages;
using DistributeMe.ImageProcessing.WPF.ViewModels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DistributeMe.ImageProcessing.WPF
{
    public class RabbitMqManager : IDisposable
    {
        private IModel channel;

        public RabbitMqManager()
        {
            var connectionFactory = new ConnectionFactory { Uri = MessagingConstants.MqUri };
            var connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();
            connection.AutoClose = true;
        }

        public void SendProcessImageCommand(IProcessImageCommand command)
        {
            channel.ExchangeDeclare(
                exchange: MessagingConstants.ProcessImageExchange,
                type: ExchangeType.Direct);


            channel.QueueDeclare(
                queue: MessagingConstants.ProcessFaceQueue, durable: false,
                exclusive: false, autoDelete: false, arguments: null);

            channel.QueueDeclare(
                queue: MessagingConstants.ProcessOcrQueue, durable: false,
                exclusive: false, autoDelete: false, arguments: null);

            channel.QueueBind(
                queue: MessagingConstants.ProcessFaceQueue,
                exchange: MessagingConstants.ProcessImageExchange,
                routingKey: "");

            channel.QueueBind(
                queue: MessagingConstants.ProcessOcrQueue,
                exchange: MessagingConstants.ProcessImageExchange,
                routingKey: "");

            var serializedCommand = JsonConvert.SerializeObject(command);

            var messageProperties = channel.CreateBasicProperties();
            messageProperties.ContentType = MessagingConstants.ContentType;

            channel.BasicPublish(
                exchange: MessagingConstants.ProcessImageExchange,
                routingKey: "",
                basicProperties: messageProperties,
                body: Encoding.UTF8.GetBytes(serializedCommand));

        }



        public void ListenForFaceProcessImageEvent(ObservableCollection<ProcessRequest> processRequests)
        {
            channel.QueueDeclare(
                queue: MessagingConstants.ProcessedFaceNotificationQueue,
                durable: false, exclusive: false,
                autoDelete: false, arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var eventingConsumer = new EventingBasicConsumer(channel);
            eventingConsumer.Received += (chan, eventArgs) =>
            {
                var contentType = eventArgs.BasicProperties.ContentType;
                if (contentType != MessagingConstants.ContentType)
                    throw new ArgumentException($"Can't handle content type {contentType}");

                var message = Encoding.UTF8.GetString(eventArgs.Body);
                var orderConsumer = new FaceRecognitionImageProcessedConsumer();
                var commandObj =
                JsonConvert.DeserializeObject<FaceRecognitionImageProcessedEvent>(message);
                orderConsumer.Consume(commandObj,processRequests);
                channel.BasicAck(deliveryTag: eventArgs.DeliveryTag,
                    multiple: false);
            };

            channel.BasicConsume(
                queue: MessagingConstants.ProcessedFaceNotificationQueue,
                noAck: false,
                consumer: eventingConsumer);
        }
        public void ListenForOcrProcessImageEvent(ObservableCollection<ProcessRequest> processRequests)
        {
            channel.QueueDeclare(
                queue: MessagingConstants.ProcessedOcrNotificationQueue,
                durable: false, exclusive: false,
                autoDelete: false, arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var eventingConsumer = new EventingBasicConsumer(channel);
            eventingConsumer.Received += (chan, eventArgs) =>
            {
                var contentType = eventArgs.BasicProperties.ContentType;
                if (contentType != MessagingConstants.ContentType)
                    throw new ArgumentException($"Can't handle content type {contentType}");

                var message = Encoding.UTF8.GetString(eventArgs.Body);
                var orderConsumer = new OcrImageProcessedConsumer();
                var commandObj =
                JsonConvert.DeserializeObject<OcrImageProcessedEvent>(message);
                orderConsumer.Consume(commandObj, processRequests);
                channel.BasicAck(deliveryTag: eventArgs.DeliveryTag,
                    multiple: false);
            };

            channel.BasicConsume(
                queue: MessagingConstants.ProcessedOcrNotificationQueue,
                noAck: false,
                consumer: eventingConsumer);
        }

        public void Dispose()
        {
            if (!channel.IsClosed)
                channel.Close();
        }
    }
}
