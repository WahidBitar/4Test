namespace Messaging.Shared
{
    public interface IMessageConsumer<T>
    {
        void Consume(byte[] messagePayload);
        void Consume(T message);
    }
}