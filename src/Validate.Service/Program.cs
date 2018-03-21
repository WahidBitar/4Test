using System;
using GreenPipes;
using GreenPipes.Configurators;
using Helpers.Core;
using MassTransit;
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
                    //e.UseRateLimit(10, TimeSpan.FromSeconds(5));
                    /*e.UseCircuitBreaker(cb =>
                    {
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                        cb.TripThreshold = 15;
                        cb.ActiveThreshold = 10;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                    });*/
                    e.Consumer<ValidateOrderCommandConsumer>();
                });
            });

            bus.Start();
        }

        private static void retryPolicy(IRetryConfigurator cfg)
        {
            //cfg.Ignore<InternalApplicationException>();
            cfg.Interval(3, TimeSpan.FromSeconds(2));
        }
    }
}