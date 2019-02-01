using System;
using System.Collections.Generic;
using Stateless;

namespace Core
{
    public class SimpleRequest
    {
        private readonly StateMachine<States, Triggers>.TriggerWithParameters<Decision> approveTrigger;
        private readonly StateMachine<States, Triggers>.TriggerWithParameters<Decision> rejectTrigger;
        private readonly StateMachine<States, Triggers> stateMachine;

        public SimpleRequest(int currentState, Person requester)
        {
            CurrentState = currentState;
            Requester = requester;

            stateMachine = new StateMachine<States, Triggers>(() => (States) CurrentState, state => CurrentState = (int) state);
            stateMachine.OnUnhandledTrigger(stateExceptionHandler);
            //stateMachine.OnTransitioned(onStateTransition);

            approveTrigger = stateMachine.SetTriggerParameters<Decision>(Triggers.Approve);
            rejectTrigger = stateMachine.SetTriggerParameters<Decision>(Triggers.Reject);

            stateMachine.Configure(States.Created)
                .PermitIf(Triggers.Post,
                    States.AwaitGroupManagerDecision,
                    () => requester.WorkPlace == Person.WorkPlaces.Group)
                .PermitIf(Triggers.Post,
                    States.AwaitDivisionManagerDecision,
                    () => requester.WorkPlace == Person.WorkPlaces.Division)
                .PermitIf(Triggers.Post,
                    States.AwaitDepartmentManagerDecision,
                    () => requester.WorkPlace == Person.WorkPlaces.Department);

            stateMachine.Configure(States.AwaitGroupManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.GroupManager))
                .OnEntryFrom(approveTrigger,onAddDecision)
                .OnEntryFrom(rejectTrigger,onAddDecision)
                .PermitIf(approveTrigger, States.AwaitDivisionManagerDecision, decision => canDecide(decision, Person.UserLevels.GroupManager))
                .PermitIf(rejectTrigger, States.Rejected, decision => canDecide(decision, Person.UserLevels.GroupManager));

            stateMachine.Configure(States.AwaitDivisionManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.DivisionManager))
                .OnEntryFrom(approveTrigger, onAddDecision)
                .OnEntryFrom(rejectTrigger, onAddDecision)
                .PermitIf(approveTrigger, States.AwaitDepartmentManagerDecision, decision => canDecide(decision, Person.UserLevels.DivisionManager))
                .PermitIf(rejectTrigger, States.Rejected, decision => canDecide(decision, Person.UserLevels.DivisionManager));

            stateMachine.Configure(States.AwaitDepartmentManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.DepartmentManager))
                .OnEntryFrom(approveTrigger, onAddDecision)
                .OnEntryFrom(rejectTrigger, onAddDecision)
                .PermitIf(approveTrigger, States.Approved, decision => canDecide(decision, Person.UserLevels.DepartmentManager))
                .PermitIf(rejectTrigger, States.Rejected, decision => canDecide(decision, Person.UserLevels.DepartmentManager));

            stateMachine.Configure(States.Approved)
                .OnEntryFrom(approveTrigger, onAddDecision)
                .OnEntry(onApprove);

            stateMachine.Configure(States.Rejected)
                .OnEntryFrom(rejectTrigger, onAddDecision)
                .OnEntry(onReject);

            stateMachine.OnTransitioned(notifyRequester);
        }

        public int CurrentState { get; private set; }
        public Person Requester { get; }
        public ICollection<Decision> Decisions { get; } = new HashSet<Decision>();

        public void Post()
        {
            stateMachine.Fire(Triggers.Post);
        }

        public void AddDecision(Decision decision)
        {
            if (decision.Result == Decision.DecisionResults.Approved)
                stateMachine.Fire(approveTrigger, decision);

            if (decision.Result == Decision.DecisionResults.Rejected)
                stateMachine.Fire(rejectTrigger, decision);            
        }

        private void onReject(StateMachine<States, Triggers>.Transition transition)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("=================================");
            Console.WriteLine($"The request of {Requester.Name} from {Requester.WorkPlace} has been Rejected");
            Console.WriteLine("=================================");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        private void onApprove(StateMachine<States, Triggers>.Transition transition)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=================================");
            Console.WriteLine($"The request of {Requester.Name} from {Requester.WorkPlace} has been Approved");
            Console.WriteLine("=================================");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        private void notifyRequester(StateMachine<States, Triggers>.Transition transition)
        {
            Console.WriteLine();
            Console.WriteLine("======== Notification to Requester ========");
            Console.WriteLine($"Hi {Requester.Name}, Your request state has been changed from {transition.Source} to {transition.Destination} after {transition.Trigger}");
            Console.WriteLine("===========================================");
            Console.WriteLine();
        }

        private bool canDecide(Decision decision, Person.UserLevels expectedLevel)
        {
            return decision.Approver != null && decision.Approver.Level == expectedLevel;
        }

        private void onAddDecision(Decision decision, StateMachine<States, Triggers>.Transition transition)
        {
            Decisions.Add(decision);
        }

        //private void onPost(StateMachine<States, Triggers>.Transition transition, Person.UserLevels approverLevel)
        private void onPost(Person.UserLevels approverLevel)
        {
            Console.WriteLine();
            Console.WriteLine("======== Notification to Approver ========");
            var approver = getManager(approverLevel);
            Console.WriteLine($"Hi, as you are {approver.Level} there is a new request awaiting your decision");
            Console.WriteLine("==========================================");
            Console.WriteLine();
        }


        private void onStateTransition(StateMachine<States, Triggers>.Transition transition)
        {
            Console.WriteLine("=======================================");
            Console.WriteLine($"The state of request has been changed from '{transition.Source}' to '{transition.Destination}' after {transition.Trigger}");
            /*Console.WriteLine("*  State to string:");
            Console.WriteLine($"   {stateMachine}");*/
            /*Console.WriteLine();
            Console.WriteLine("*  State Uml Dot Graph:");
            Console.WriteLine(UmlDotGraph.Format(stateMachine.GetInfo()));*/
            Console.WriteLine("=======================================");
            Console.WriteLine();
        }

        private void stateExceptionHandler(States state, Triggers trigger, ICollection<string> args)
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

        private Person getManager(Person.UserLevels userLevel)
        {
            var result = new Person {Level = userLevel};
            switch (userLevel)
            {
                case Person.UserLevels.GroupManager:
                    result.WorkPlace = Person.WorkPlaces.Group;
                    result.Name = "G.M Sam";
                    break;
                case Person.UserLevels.DivisionManager:
                    result.WorkPlace = Person.WorkPlaces.Division;
                    result.Name = "Dv.M Ham";
                    break;
                case Person.UserLevels.DepartmentManager:
                    result.WorkPlace = Person.WorkPlaces.Department;
                    result.Name = "Dp.M Yat";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(userLevel), userLevel, null);
            }

            return result;
        }


        private enum Triggers
        {
            Post,
            Approve,
            Reject
        }

        private enum States
        {
            Created = 1,
            AwaitGroupManagerDecision = 2,
            AwaitDivisionManagerDecision = 3,
            AwaitDepartmentManagerDecision = 4,
            Approved = 5,
            Rejected = 6
        }
    }
}