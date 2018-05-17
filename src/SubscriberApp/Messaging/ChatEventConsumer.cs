using System;
using System.Collections.Generic;
using System.Text;
using Messaging.Shared;
using Newtonsoft.Json;

namespace SubscriberApp.Messaging
{
    public class ChatEventConsumer : IMessageConsumer<ChatEvent>
    {
        public void Consume(byte[] messagePayload)
        {
            var payloadString = Encoding.UTF8.GetString(messagePayload);
            var message = JsonConvert.DeserializeObject<ChatEvent>(payloadString);
            Consume(message);
        }

        public void Consume(ChatEvent message)
        {
            Console.WriteLine($"new message from: {message.SenderName}");
            Console.WriteLine($"  - {message.MessageText}");
            Console.WriteLine();
        }
    }
}
