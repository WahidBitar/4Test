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

            stateMachine.OnUnhandledTrigger(stateExceptionHandler);

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
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("=================================");
            Console.WriteLine($"The request of {Requester.Name} from {Requester.WorkPlace} has been Rejected");
            Console.WriteLine("=================================");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        protected virtual void OnApprove(StateMachine<RequestState, Triggers>.Transition transition)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=================================");
            Console.WriteLine($"The request of {Requester.Name} from {Requester.WorkPlace} has been Approved");
            Console.WriteLine("=================================");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }        

        protected void stateExceptionHandler(RequestState state, Triggers trigger, ICollection<string> args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"It's not allowed to {trigger} on the current state '{state}'");
            if (args != null)
            {
                Console.WriteLine("The passed args");
                foreach (var arg in args) Console.WriteLine(arg);
            }

            Console.ForegroundColor = ConsoleColor.White;
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