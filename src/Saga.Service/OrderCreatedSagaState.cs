using System;
using System.Collections.Generic;
using Automatonymous;
using MassTransit.MongoDbIntegration.Saga;

namespace Saga.Service
{
    public class OrderCreatedSagaState : SagaStateMachineInstance, IVersionedSaga
    {
        public OrderCreatedSagaState(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public OrderCreatedSagaState()
        {
            
        }
        public int Version { get; set; }
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public string OrderId { get; set; }
        public string Text { get; set; }
        public DateTime CreateDate { get; set; }
        public string RemainingServices { get; set; }

        /*public int RequestFinishedStatusBits { get; set; }

        public CompositeEventStatus RequestFinishedStatus
        {
            get { return new CompositeEventStatus(RequestFinishedStatusBits); }
            set { RequestFinishedStatusBits = value.Bits; }
        }*/

    }
}