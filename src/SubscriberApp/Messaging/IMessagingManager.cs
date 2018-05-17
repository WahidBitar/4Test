using Messaging.Shared;

namespace SubscriberApp.Messaging
{
    public interface IMessagingManager
    {
        void ListenForChatMessageEvent();
        void ListenForChatMessageRetryEvent();
        void PublishErrorChatEventMessage(ChatEvent message);
        void PublishRetryChatEventMessage(ChatEvent message);
    }
}