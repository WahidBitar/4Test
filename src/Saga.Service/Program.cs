using System;
using System.Reflection.Emit;
using Automatonymous;
using GreenPipes;
using GreenPipes.Configurators;
using Helpers.Core;
using MassTransit;
using MassTransit.MongoDbIntegration.Saga;
using MassTransit.Saga;
using Message.Contracts;
using MongoDB.Driver;

namespace Saga.Service
{
    class Program
    {
        private static OrderCreatedStateMachine machine;

        private static ISagaRepository<OrderCreatedSagaState> repository;
        //private static Lazy<ISagaRepository<OrderCreatedSagaState>> lazyRepository;

        static void Main(string[] args)
        {
            machine = new OrderCreatedStateMachine();
            repository = new InMemorySagaRepository<OrderCreatedSagaState>();
            //SagaDbContextFactory sagaDbContextFactory = () => new SagaDbContext<OrderCreatedSagaState, OrderCreatedSagaSagaMap>(SagaDbContextFactoryProvider.ConnectionString);
            //lazyRepository = new Lazy<ISagaRepository<OrderCreatedSagaState>>(() => new EntityFrameworkSagaRepository<OrderCreatedSagaState>(sagaDbContextFactory));
            //var database = new MongoClient("mongodb://localhost").GetDatabase("OrderManagement");
            //var mongoRepository = new MongoDbSagaRepository<OrderCreatedSagaState>(MongoUrl.Create("mongodb://localhost/OrderManagement"));
            var bus = BusConfigurator.ConfigureBus(MessagingConstants.MqUri, MessagingConstants.UserName, MessagingConstants.Password, (cfg, host) =>
            {
                cfg.ReceiveEndpoint(host, MessagingConstants.SagaQueue, e =>
                {
                    //e.UseRetry(retryPolicy);
                    e.StateMachineSaga(machine, repository);
                    //e.StateMachineSaga(machine, mongoRepository);
                });
            });
            //bus.ConnectConsumeObserver(new ConsumeObserver());
            //bus.ConnectConsumeMessageObserver(new ViolationObserver());
            bus.Start();
        }

        /*private static void retryPolicy(IRetryConfigurator cfg)
        {
            cfg.Ignore<InternalApplicationException>();
            cfg.Interval(7, TimeSpan.FromSeconds(3));
        }*/
    }

/*    public class OrderCreatedSagaSagaMap : SagaClassMapping<OrderCreatedSagaState>
    {
        public OrderCreatedSagaSagaMap()
        {
            Property(x => x.CurrentState).HasMaxLength(64);
            Property(x => x.OrderId);
            Property(x => x.CreateDate);
            Property(x => x.Text);
            Property(x => x.RemainingServices);
            //Property(x => x.RequestFinishedStatusBits);
        }
    }*/
}






