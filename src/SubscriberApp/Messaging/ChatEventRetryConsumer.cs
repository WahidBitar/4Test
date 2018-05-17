using System;
using System.Collections.Generic;
using System.Text;
using Messaging.Shared;
using Newtonsoft.Json;

namespace SubscriberApp.Messaging
{
    public class ChatEventRetryConsumer : IMessageRetryConsumer<ChatEvent>
    {
        private readonly IMessagingManager messagingManager;

        public ChatEventRetryConsumer(IMessagingManager messagingManager)
        {
            this.messagingManager = messagingManager;
        }

        public void Consume(byte[] messagePayload, int previousAttempts)
        {
            try
            {
                var payloadString = Encoding.UTF8.GetString(messagePayload);
                var message = JsonConvert.DeserializeObject<ChatEvent>(payloadString);
                Consume(message, previousAttempts);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Consume(ChatEvent message, int previousAttempts)
        {
            try
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message), "The message was null");
                }
                else if (message.MessageText == "fake" && previousAttempts < 2)
                {
                    messagingManager.PublishRetryChatEventMessage(message, previousAttempts);
                }
                else
                {
                    Console.WriteLine($"got a message after {previousAttempts + 1} retries from: {message.SenderName}");
                    Console.WriteLine($"  --- {message.MessageText}");
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