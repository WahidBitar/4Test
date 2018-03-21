using System;
using Automatonymous;

namespace Saga.Service
{
    public class OrderCreatedSagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public string OrderId { get; set; }
        public string Text { get; set; }
        public DateTime CreateDate { get; set; }
        public string RemainingServices { get; set; }
    }
}