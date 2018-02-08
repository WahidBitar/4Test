using System;
using System.Threading.Tasks;
using Helpers.Core;
using MassTransit;
using Message.Contracts;

namespace Validate.Service
{
    public class ValidateOrderCommandConsumer : IConsumer<IValidateOrderCommand>
    {
        private Random random;

        public ValidateOrderCommandConsumer()
        {
            random = new Random();
        }

        public async Task Consume(ConsumeContext<IValidateOrderCommand> context)
        {
            var violationHandler = new ViolationHandler<IValidateOrderCommand>();
            var command = context.Message;
            var startProcessTime = DateTime.UtcNow;

            try
            {
                if (command == null)
                    throw new InternalApplicationException<IValidateOrderCommand>(x => x, ViolationType.Null);

                if (string.IsNullOrWhiteSpace(command.OriginalText))
                    throw new InternalApplicationException<IValidateOrderCommand>(x => x.OriginalText, ViolationType.Required);

                if (random.Next(1, 9) % 2 == 0)
                    throw new Exception("Bad luck!. Try again :P");

                if (command.OriginalText == "asd")
                    violationHandler.AddViolation(x => x.OriginalText, ViolationType.NotAllowed);

                if (command.OriginalText.Contains("123"))
                    violationHandler.AddViolation(x => x.OriginalText, ViolationType.Invalid);

                if (!violationHandler.IsValid)
                    throw new InternalApplicationException(violationHandler.Violations);

                await context.Publish<IValidateOrderResponse>(new ValidateOrderResponse(command.OrderId)
                {
                    StartProcessTime = startProcessTime,
                    EndProcessTime = DateTime.UtcNow,
                });
            }
            catch (InternalApplicationException ex)
            {
                await context.Publish<IValidateOrderResponse>(new ValidateOrderResponse(command.OrderId, ex.Violations)
                    {
                        StartProcessTime = startProcessTime,
                        EndProcessTime = DateTime.UtcNow,
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                violationHandler.AddViolation(x => x, message: ex.Message);
                await context.Publish<IValidatedMessage>(new ValidatedMessage(command.OrderId, violationHandler.Violations));
                throw;
            }
        }
    }
}