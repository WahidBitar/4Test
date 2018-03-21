using Automatonymous;
using Helpers.Core;
using MassTransit;
using MassTransit.Saga;
using Message.Contracts;

namespace Saga.Service
{
    class Program
    {
        private static OrderCreatedStateMachine machine;

        private static ISagaRepository<OrderCreatedSagaState> repository;

        static void Main(string[] args)
        {
            machine = new OrderCreatedStateMachine();
            repository = new InMemorySagaRepository<OrderCreatedSagaState>();
            var bus = BusConfigurator.ConfigureBus(MessagingConstants.MqUri, MessagingConstants.UserName, MessagingConstants.Password, (cfg, host) =>
            {
                cfg.ReceiveEndpoint(host, MessagingConstants.SagaQueue, e =>
                {
                    e.StateMachineSaga(machine, repository);
                });
            });
            bus.Start();
        }
    }
}






