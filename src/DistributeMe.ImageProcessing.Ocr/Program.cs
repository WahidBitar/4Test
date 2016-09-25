﻿using System;
using System.ServiceProcess;
using DistributeMe.ImageProcessing.Messaging;
using MassTransit;

namespace DistributeMe.ImageProcessing.Ocr
{
    public static class Program
    {
        private static IBusControl bus;

        #region Nested classes to support running as service

        public const string ServiceName = "ImageOcrRecognitionService";

        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start(args);
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }

        #endregion

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
                // running as service
                using (var service = new Service())
                    ServiceBase.Run(service);
            else
            {
                // running as console app
                Start(args);

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);

                Stop();
            }
        }

        private static void Start(string[] args)
        {
            Console.Title = ServiceName;

            bus = BusConfigurator.ConfigureBus((cfg, host) =>
            {
                cfg.ReceiveEndpoint(host, MessagingConstants.ProcessOcrQueue, e =>
                {
                    e.Consumer<ProcessOcrConsumer>();
                });
            });
            bus.Start();

            Console.WriteLine("Listening for Process Image Command to do OCR..");
            Console.ReadKey(true);
        }

        private static void Stop()
        {
            bus.Stop();
        }
    }


}
