using System;
using System.Linq;
using System.Threading.Tasks;
using Automatonymous;
using Automatonymous.Binders;
using Message.Contracts;

namespace Saga.Service
{
    public class OrderCreatedStateMachine : MassTransitStateMachine<OrderCreatedSagaState>
    {
        public OrderCreatedStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderCreated, x => x.CorrelateBy(cart => cart.OrderId.ToString(), context => context.Message.OrderId.ToString())
                .SelectId(context => context.Message.OrderId /*Guid.NewGuid()*/));

            Event(() => ValidateOrderResponse, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => NormalizeOrderResponse, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => CapitalizeOrderResponse, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderReadyToProcessEvent, x => x.CorrelateById(context => context.Message.OrderId));

            Event(() => ValidatedMessageReceived, x => x.CorrelateById(context => context.Message.CorrelationId));

            /*Event(() => ViolationOccurredEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => ValidateOrderCommandFailed, x => x.CorrelateById(context => context.Message.Message.OrderId));*/


            Initially(
                When(OrderCreated, shouldValidate)
                    .Then(updateState)
                    .TransitionTo(Active)
                    .Publish(args => new ValidateOrderCommand
                    {
                        OriginalText = args.Data.OriginalText,
                        OrderId = args.Data.OrderId,
                    }),

                When(OrderCreated, context => !shouldValidate(context))
                    .Then(updateState)
                    .TransitionTo(NoValidationRequired)
                    .Publish(context => new OrderReadyToProcessEvent
                    {
                        OrderId = context.Data.OrderId,
                        OriginalText = context.Data.OriginalText,
                        Services = context.Data.Services,
                    })
            );

            DuringAny(
                When(ValidatedMessageReceived, context => !context.Data.IsValid)
                    .Then(context => { context.Instance.RemainingServices = ""; })
                    .TransitionTo(Failed)
                    .Publish(context => new OrderValidatedEvent(context.Data.CorrelationId, context.Data.Violations))
                    .Finalize()
            );


            During(Active,
                When(ValidateOrderResponse, context => context.Data.IsValid)
                    .Then(context => { context.Instance.RemainingServices = string.Join("|", context.Instance.RemainingServices.Split('|').Where(s => s != "Validate")); })
                    .TransitionTo(Validated)
                    .Publish(context => new OrderReadyToProcessEvent
                    {
                        OrderId = context.Data.OrderId,
                        OriginalText = context.Instance.Text,
                        Services = context.Instance.RemainingServices.Split('|'),
                    }),
                When(ValidateOrderResponse)
                    .Publish(context => new OrderValidatedEvent(context.Data.OrderId, context.Data.Violations)
                    {
                        StartProcessTime = context.Data.StartProcessTime,
                        EndProcessTime = context.Data.EndProcessTime,
                    })
            );


            During(Validated, NoValidationRequired,
                When(OrderReadyToProcessEvent, context => context.Instance.RemainingServices.Contains("Normalize"))
                    .Publish(args => new NormalizeOrderCommand
                    {
                        OriginalText = args.Data.OriginalText,
                        OrderId = args.Data.OrderId
                    }),
                When(OrderReadyToProcessEvent, context => context.Instance.RemainingServices.Contains("Capitalize"))
                    .Publish(args => new CapitalizeOrderCommand
                    {
                        OriginalText = args.Data.OriginalText,
                        OrderId = args.Data.OrderId
                    })
            );


            During(Validated, NoValidationRequired,
                When(NormalizeOrderResponse)
                    .Then(context =>
                    {
                        context.Instance.RemainingServices = string.Join("|", context.Instance.RemainingServices.Split('|').Where(s => s != "Normalize"));
                        context.Instance.Text = context.Data.NormalizedText;
                    })
                    .Publish(context => new OrderNormalized(context.Data.OrderId)
                    {
                        NormalizedText = context.Data.NormalizedText,
                        ProcessTime = (context.Data.EndProcessTime - context.Data.StartProcessTime).Milliseconds
                    }),
                When(CapitalizeOrderResponse)
                    .Then(context =>
                    {
                        context.Instance.RemainingServices = string.Join("|", context.Instance.RemainingServices.Split('|').Where(s => s != "Capitalize"));
                        context.Instance.Text = context.Data.CapitalizeText;
                    })
                    .Publish(context => new OrderCapitalized(context.Data.OrderId)
                    {
                        CapitalizedText = context.Data.CapitalizeText,
                        ProcessTime = (context.Data.EndProcessTime - context.Data.StartProcessTime).Milliseconds
                    }),
                When(NormalizeOrderResponse, context => !context.Instance.RemainingServices.Any())
                    .TransitionTo(Finished)
                    .Finalize(),
                When(CapitalizeOrderResponse, context => !context.Instance.RemainingServices.Any())
                    .TransitionTo(Finished)
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }

        private Task onUnhandledEvent(UnhandledEventContext<OrderCreatedSagaState> context)
        {
            Console.WriteLine($"Unhandled Event: {context.Event.Name}");
            return Task.CompletedTask;
        }

        private ExceptionActivityBinder<OrderCreatedSagaState, IValidatedMessage, Exception> handleExceptions(ExceptionActivityBinder<OrderCreatedSagaState, IValidatedMessage, Exception> builder)
        {
            return builder.Then(context =>
                {
                    context.Instance.RemainingServices = "";
                    Console.WriteLine(context.Exception);
                })
                .TransitionTo(Failed)
                .Finalize();
        }


        public State Active { get; private set; }

        public State NoValidationRequired { get; private set; }

        public State Validated { get; private set; }

        public State Failed { get; private set; }

        public State Finished { get; private set; }


        public Event<IOrderCreatedEvent> OrderCreated { get; set; }

        public Event<IOrderReadyToProcessEvent> OrderReadyToProcessEvent { get; set; }

        public Event<IValidateOrderResponse> ValidateOrderResponse { get; set; }

        public Event<INormalizeOrderResponse> NormalizeOrderResponse { get; set; }

        public Event<ICapitalizeOrderResponse> CapitalizeOrderResponse { get; set; }

        public Event<IValidatedMessage> ValidatedMessageReceived { get; set; }


        /*public Event<IViolationOccurredEvent> ViolationOccurredEvent { get; set; }
        public Event<Fault<IValidateOrderCommand>> ValidateOrderCommandFailed { get; set; }*/

        private bool shouldValidate(EventContext<OrderCreatedSagaState, IOrderCreatedEvent> context)
        {
            return context.Data.Services.Any(s => s == "Validate");
        }

        private void updateState(BehaviorContext<OrderCreatedSagaState, IOrderCreatedEvent> context)
        {
            context.Instance.OrderId = context.Data.OrderId;
            context.Instance.Text = context.Data.OriginalText;
            context.Instance.CreateDate = context.Data.CreateDate;
            context.Instance.RemainingServices = string.Join("|", context.Data.Services);
        }
    }
}