using Messaging.Shared;

namespace SubscriberApp.Messaging
{
    public interface IMessagingManager
    {
        void ListenForChatMessageEvent();
        void ListenForChatMessageRetryEvent();
        void PublishRetryChatEventMessage(ChatEvent message);
        void PublishErrorChatEventMessage(ChatEvent message);
    }
}