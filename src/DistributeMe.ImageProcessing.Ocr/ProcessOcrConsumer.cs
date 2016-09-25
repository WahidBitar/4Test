﻿using System;
using System.Threading;
using System.Threading.Tasks;
using DistributeMe.ImageProcessing.Messaging;
using DistributeMe.ImageProcessing.Ocr.Messages;
using MassTransit;

namespace DistributeMe.ImageProcessing.Ocr
{
    internal class ProcessOcrConsumer : IConsumer<IProcessImageCommand>
    {
        public async Task Consume(ConsumeContext<IProcessImageCommand> context)
        {
            var command = context.Message;

            await Console.Out.WriteLineAsync($"Processing Request: {command.RequestId}");

            var processStartDate = DateTime.UtcNow;
            Thread.Sleep(1500);
            //await Task.Run(() => Thread.Sleep(1500));

            await Console.Out.WriteLineAsync($"DONE");

            var notificationEvent = new OcrImageProcessedEvent(command.RequestId, "extracted text", processStartDate, DateTime.UtcNow);
            await context.Publish<IOcrImageProcessedEvent>(notificationEvent);
        }
    }
}