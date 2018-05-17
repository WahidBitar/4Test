﻿using System;
using System.Collections.Generic;
using System.Text;
using Messaging.Shared;
using Newtonsoft.Json;

namespace SubscriberApp.Messaging
{
    public class ChatEventConsumer : IMessageConsumer<ChatEvent>
    {
        private readonly IMessagingManager messagingManager;

        public ChatEventConsumer(IMessagingManager messagingManager)
        {
            this.messagingManager = messagingManager;
        }

        public void Consume(byte[] messagePayload)
        {
            try
            {
                var payloadString = Encoding.UTF8.GetString(messagePayload);
                var message = JsonConvert.DeserializeObject<ChatEvent>(payloadString);
                Consume(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Consume(ChatEvent message)
        {
            try
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message), "The message was null");
                }
                else if (message.MessageText == "fake")
                {
                    messagingManager.PublishRetryChatEventMessage(message);
                }
                else
                {
                    Console.WriteLine($"new message from: {message.SenderName}");
                    Console.WriteLine($"  - {message.MessageText}");
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                messagingManager.PublishErrorChatEventMessage(message);
            }
        }
    }
}