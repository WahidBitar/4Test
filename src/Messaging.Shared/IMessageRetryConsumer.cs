namespace Messaging.Shared
{
    public interface IMessageRetryConsumer<T>
    {
        void Consume(byte[] messagePayload, int previousAttempts);
        void Consume(T message, int previousAttempts);
    }
}