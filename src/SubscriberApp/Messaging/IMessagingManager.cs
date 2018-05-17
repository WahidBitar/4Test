namespace SubscriberApp.Messaging
{
    public interface IMessagingManager
    {
        void ListenForChatMessageEvent();
        void ListenForChatMessageRetryEvent();
    }
}