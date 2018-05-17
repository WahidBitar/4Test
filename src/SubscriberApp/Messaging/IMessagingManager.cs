using Messaging.Shared;

namespace SubscriberApp.Messaging
{
    public interface IMessagingManager
    {
        void ListenForChatMessageEvent();
        void ListenForChatMessageRetryEvent();
        void PublishRetryChatEventMessage(ChatEvent message, int retryAttempts = 0);
        void PublishErrorChatEventMessage(ChatEvent message);
    }
}