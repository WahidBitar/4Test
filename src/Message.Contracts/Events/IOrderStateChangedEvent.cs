using System;

namespace Message.Contracts
{
    public interface IOrderStateChangedEvent
    {
        Guid OrderId { get; }
        string State { get; }
    }

    public class OrderStateChangedEvent : IOrderStateChangedEvent
    {
        public OrderStateChangedEvent(Guid orderId, string state)
        {
            OrderId = orderId;
            State = state;
        }

        public Guid OrderId { get; }
        public string State { get; }
    }
}