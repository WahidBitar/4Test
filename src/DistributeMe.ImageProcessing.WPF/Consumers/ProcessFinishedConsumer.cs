﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DistributeMe.ImageProcessing.Messaging;
using DistributeMe.ImageProcessing.WPF.ViewModels;
using MassTransit;

namespace DistributeMe.ImageProcessing.WPF.Consumers
{
    public class ProcessFinishedConsumer : IConsumer<IProcessRequestFinishedEvent>
    {
        private readonly ObservableCollection<ProcessRequest> processRequests;

        public ProcessFinishedConsumer(ObservableCollection<ProcessRequest> processRequests)
        {
            this.processRequests = processRequests;
        }

        public async Task Consume(ConsumeContext<IProcessRequestFinishedEvent> context)
        {
            var command = context.Message;

            var request = processRequests.FirstOrDefault(r => r.RequestId == command.RequestId);
            if (request == null)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                request.Notifications.Insert(0, $"Request Processing Finished");
            });
        }
    }
}