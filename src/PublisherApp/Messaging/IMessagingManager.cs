using Messaging.Shared;

namespace PublisherApp.Messaging
{
    public interface IMessagingManager
    {
        void PublishChatEventMessage(ChatEvent message);
    }
}