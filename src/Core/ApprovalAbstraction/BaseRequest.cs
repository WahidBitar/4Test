using System;
using System.Collections.Generic;
using Stateless;

namespace Core.ApprovalAbstraction
{

    public abstract class BaseRequest
    {
        protected StateMachine<RequestState, Triggers>.TriggerWithParameters<Decision> approveTrigger;
        protected StateMachine<RequestState, Triggers>.TriggerWithParameters<Decision> askForModificationTrigger;
        protected StateMachine<RequestState, Triggers>.TriggerWithParameters<Decision> rejectTrigger;
        protected StateMachine<RequestState, Triggers> stateMachine;

        public BaseRequest(RequestState currentState, Person requester)
        {
            CurrentState = currentState;
            Requester = requester;

            stateMachine = new StateMachine<RequestState, Triggers>(() => CurrentState, state => CurrentState = state);

            stateMachine.OnUnhandledTrigger(StateExceptionHandler);

            approveTrigger = stateMachine.SetTriggerParameters<Decision>(Triggers.Approve);
            rejectTrigger = stateMachine.SetTriggerParameters<Decision>(Triggers.Reject);
            askForModificationTrigger = stateMachine.SetTriggerParameters<Decision>(Triggers.AskForModification);

            stateMachine.Configure(RequestState.Approved)
                .OnEntry(OnApprove);

            stateMachine.Configure(RequestState.Rejected)
                .OnEntry(OnReject);
        }

        public RequestState CurrentState { get; set; }
        public Person Requester { get; set; }
        public ICollection<Decision> Decisions { get; } = new HashSet<Decision>();

        public virtual void Post()
        {
            stateMachine.Fire(Triggers.Post);
        }

        public virtual void AddDecision(Decision decision)
        {
            if (decision.Result == Decision.DecisionResults.Approved)
                stateMachine.Fire(approveTrigger, decision);

            if (decision.Result == Decision.DecisionResults.Rejected)
                stateMachine.Fire(rejectTrigger, decision);

            if (decision.Result == Decision.DecisionResults.AskForModification)
                stateMachine.Fire(askForModificationTrigger, decision);
        }

        //protected abstract void WorkflowSetup(StateMachine<RequestState, Triggers> machine);

        protected virtual void OnReject(StateMachine<RequestState, Triggers>.Transition transition)
        {
            Helpers.OnRejectNotification(Requester.Name, Requester.WorkPlace);
        }

        protected virtual void OnApprove(StateMachine<RequestState, Triggers>.Transition transition)
        {
            Helpers.OnApproveNotification(Requester.Name, Requester.WorkPlace);
        }

        protected virtual void StateExceptionHandler(RequestState state, Triggers trigger, ICollection<string> args)
        {
            Helpers.HandleException(state, trigger, args);
        }


        protected enum Triggers
        {
            Post = 1,
            Approve = 2,
            Reject = 3,
            AskForModification = 4
        }
    }

}