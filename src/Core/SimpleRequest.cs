using System;
using System.Collections.Generic;
using Stateless;

namespace Core
{
    public class SimpleRequest
    {
        private readonly StateMachine<States, Triggers>.TriggerWithParameters<Decision> decisionTrigger;
        private readonly StateMachine<States, Triggers> stateMachine;

        public SimpleRequest(int currentState, Person requester)
        {
            CurrentState = currentState;
            Requester = requester;

            stateMachine = new StateMachine<States, Triggers>(() => (States) CurrentState, state => CurrentState = (int) state);
            stateMachine.OnUnhandledTrigger(stateExceptionHandler);
            stateMachine.OnTransitioned(onStateTransition);

            decisionTrigger = stateMachine.SetTriggerParameters<Decision>(Triggers.AddDecision);

            stateMachine.Configure(States.Created)
                .PermitIf(Triggers.Post,
                    States.AwaitGroupManagerDecision,
                    () => requester.WorkPlace == Person.WorkPlaces.Group)
                .PermitIf(Triggers.Post,
                    States.AwaitDivisionManagerDecision,
                    () => requester.WorkPlace == Person.WorkPlaces.Division)
                .PermitIf(Triggers.Post,
                    States.AwaitDepartmentManagerDecision,
                    () => requester.WorkPlace == Person.WorkPlaces.Department)
                .OnExit(notifyRequester);

            stateMachine.Configure(States.AwaitGroupManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.GroupManager))
                .PermitIf(decisionTrigger,
                    States.AwaitDivisionManagerDecision,
                    decision => decision.Approver != null && decision.Approver.Level == Person.UserLevels.DivisionManager);

            stateMachine.Configure(States.AwaitDivisionManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.DivisionManager))
                .PermitIf(decisionTrigger,
                    States.AwaitDepartmentManagerDecision,
                    decision => decision.Approver != null && decision.Approver.Level == Person.UserLevels.DepartmentManager);

            stateMachine.Configure(States.AwaitDepartmentManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.DepartmentManager))
                .PermitIf(decisionTrigger, States.Approved, decision => decision.Result == Decision.DecisionResults.Approved)
                .PermitIf(decisionTrigger, States.Rejected, decision => decision.Result == Decision.DecisionResults.Rejected)
                .OnExit(notifyRequester);
        }

        private void notifyRequester(StateMachine<States, Triggers>.Transition transition)
        {
            Console.WriteLine("======== Notification to Requester ========");
            Console.WriteLine($"Hi {Requester.Name}, Your request state has been changed from {transition.Source} to {transition.Destination} after {transition.Trigger}");
            Console.WriteLine("==============================");
            Console.WriteLine();
        }

        public int CurrentState { get; set; }
        public Person Requester { get; }
        public ICollection<Decision> Decisions { get; set; } = new HashSet<Decision>();

        public void Post()
        {
            stateMachine.Fire(Triggers.Post);
        }

        public void AddDecision(Decision decision)
        {
            stateMachine.Fire(decisionTrigger, decision);
        }

        private void onAddDecision(Decision decision, StateMachine<States, Triggers>.Transition transition)
        {
            Decisions.Add(decision);
        }

        //private void onPost(StateMachine<States, Triggers>.Transition transition, Person.UserLevels approverLevel)
        private void onPost(Person.UserLevels approverLevel)
        {
            Console.WriteLine("======== Notification to Approver ========");
            var approver = new Person
            {
                Name = "SAMEER",
                Level = approverLevel
            };
            var decisionNumber = Helpers.GetNumber();
            Decisions.Add(new Decision
            {
                Id = decisionNumber,
                Approver = approver,
                Result = Decision.DecisionResults.Undetermined
            });

            Console.WriteLine($"Hi, as you are {approver.Level} there is a new request awaiting your decision with Ticket number {decisionNumber}");
            Console.WriteLine("==============================");
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
        }


        private enum Triggers
        {
            Post,
            AddDecision
        }

        private enum States
        {
            Created = 1,
            AwaitGroupManagerDecision = 2,
            AwaitDivisionManagerDecision = 3,
            AwaitDepartmentManagerDecision = 4,
            Approved = 5,
            Rejected = 6,
        }

        /*private Person.UserLevels getApproverByWorkplace(Person.WorkPlaces workPlace)
{
    switch (workPlace)
    {
        case Person.WorkPlaces.Group:
            return Person.UserLevels.GroupManager;
        case Person.WorkPlaces.Division:
            return Person.UserLevels.DivisionManager;
        case Person.WorkPlaces.Department:
            return Person.UserLevels.DepartmentManager;
        default:
            throw new ArgumentOutOfRangeException(nameof(workPlace), workPlace, null);
    }
}*/
    }
}