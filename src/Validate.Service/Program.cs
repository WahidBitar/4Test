using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenPipes;
using GreenPipes.Configurators;
using Helpers.Core;
using MassTransit;
using MassTransit.Saga;
using Message.Contracts;

namespace Validate.Service
{
    class Program
    {
        static void Main(string[] args)
        {

            var bus = BusConfigurator.ConfigureBus(MessagingConstants.MqUri, MessagingConstants.UserName, MessagingConstants.Password, (cfg, host) =>
            {
                //cfg.UseRetry(retryPolicy);

                cfg.ReceiveEndpoint(host, MessagingConstants.ValidateServiceQueue, e =>
                {
                    e.Consumer<ValidateOrderCommandConsumer>();
                });
            });

            bus.Start();
        }

        private static void retryPolicy(IRetryConfigurator cfg)
        {
            //cfg.Ignore<InternalApplicationException>();
            cfg.Interval(3, TimeSpan.FromSeconds(1));
        }
    }
}