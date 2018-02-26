using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Automatonymous;
using Automatonymous.Activities;
using Automatonymous.Binders;
using MassTransit;
using MassTransit.MongoDbIntegration.Saga;
using Message.Contracts;

namespace Saga.Service
{
    public class OrderCreatedStateMachine : MassTransitStateMachine<OrderCreatedSagaState>
    {
        public OrderCreatedStateMachine()
        {
            InstanceState(x => x.CurrentState);
            
            Event(() => OrderCreated, x => x.CorrelateBy(cart => cart.OrderId, context => context.Message.OrderId.ToString())
                .SelectId(context => context.Message.OrderId));

            Event(() => ValidateOrderResponse, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => NormalizeOrderResponse, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => CapitalizeOrderResponse, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderReadyToProcessEvent, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => ValidatedMessageReceived, x => x.CorrelateById(context => context.Message.CorrelationId));
            


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

            // To just publish event on any change in the state
            //WhenLeaveAny(publishActivityOnChangeState);

            // To do any kind of activity when state change
            WhenEnterAny(actionActivityOnChangeState);

            SetCompletedWhenFinalized();
        }

        private EventActivityBinder<OrderCreatedSagaState> publishActivityOnChangeState(EventActivityBinder<OrderCreatedSagaState> arg)
        {
            var result = arg.Add(new PublishActivity<OrderCreatedSagaState, IOrderStateChangedEvent>(context => new OrderStateChangedEvent(context.Instance.CorrelationId, context.Instance.CurrentState)));
            return result;
        }

        private EventActivityBinder<OrderCreatedSagaState> actionActivityOnChangeState(EventActivityBinder<OrderCreatedSagaState> arg)
        {
            var result = arg.Add(new ActionActivity<OrderCreatedSagaState>(context =>
            {
                //if (context.Instance.CurrentState != Final.Name && context.Instance.CurrentState != Initial.Name)
                    context.Publish(new OrderStateChangedEvent(context.Instance.CorrelationId, context.Instance.CurrentState));
            }));
            return result;
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

        private bool shouldValidate(EventContext<OrderCreatedSagaState, IOrderCreatedEvent> context)
        {
            return context.Data.Services.Any(s => s == "Validate");
        }

        private void updateState(BehaviorContext<OrderCreatedSagaState, IOrderCreatedEvent> context)
        {
            context.Instance.OrderId = context.Data.OrderId.ToString();
            context.Instance.Text = context.Data.OriginalText;
            context.Instance.CreateDate = context.Data.CreateDate;
            context.Instance.RemainingServices = string.Join("|", context.Data.Services);
        }
    }
}