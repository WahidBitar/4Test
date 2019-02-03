using System;
using Stateless;

namespace Core.ApprovalAbstraction
{
    public class AnotherRequest : BaseRequest
    {
        public AnotherRequest(RequestState currentState, Person requester) : base(currentState,requester)
        {
            stateMachine.Configure(RequestState.Created)
                .PermitIf(Triggers.Post,
                    States.AwaitGroupManagerDecision,
                    () => Requester.WorkPlace == Person.WorkPlaces.Group)
                .PermitIf(Triggers.Post,
                    States.AwaitDepartmentManagerDecision,
                    () => Requester.WorkPlace == Person.WorkPlaces.Department);

            stateMachine.Configure(States.NeedModification)
                .SubstateOf(RequestState.Created)
                .OnEntryFrom(askForModificationTrigger, onAddDecision)
                .OnEntryFrom(askForModificationTrigger, modificationNeeded);

            stateMachine.Configure(States.AwaitGroupManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.GroupManager))
                .OnEntryFrom(approveTrigger, onAddDecision)
                .OnEntryFrom(rejectTrigger, onAddDecision)
                .PermitIf(askForModificationTrigger, States.NeedModification, decision => canDecide(decision, Person.UserLevels.GroupManager))
                .PermitIf(approveTrigger, States.AwaitDepartmentManagerDecision, decision => canDecide(decision, Person.UserLevels.GroupManager))
                .PermitIf(rejectTrigger, RequestState.Rejected, decision => canDecide(decision, Person.UserLevels.GroupManager));

            stateMachine.Configure(States.AwaitDepartmentManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.DepartmentManager))
                .OnEntryFrom(approveTrigger, onAddDecision)
                .OnEntryFrom(rejectTrigger, onAddDecision)
                .PermitIf(askForModificationTrigger, States.NeedModification, decision => canDecide(decision, Person.UserLevels.DepartmentManager))
                .PermitIf(approveTrigger, RequestState.Approved, decision => canDecide(decision, Person.UserLevels.DepartmentManager))
                .PermitIf(rejectTrigger, RequestState.Rejected, decision => canDecide(decision, Person.UserLevels.DepartmentManager));

            stateMachine.Configure(RequestState.Approved)
                .OnEntryFrom(approveTrigger, onAddDecision);

            stateMachine.Configure(RequestState.Rejected)
                .OnEntryFrom(rejectTrigger, onAddDecision);

            stateMachine.OnTransitioned(notifyRequester);
        }

        #region Private Methods

        private void onPost(Person.UserLevels approverLevel)
        {
            Console.WriteLine();
            Console.WriteLine("======== Notification to Approver ========");
            var approver = Helpers.GetManager(approverLevel);
            Console.WriteLine($"Hi, as you are {approver.Level} there is a new request awaiting your decision");
            Console.WriteLine("==========================================");
            Console.WriteLine();
        }

        private bool canDecide(Decision decision, Person.UserLevels expectedLevel)
        {
            return decision.Approver != null && decision.Approver.Level == expectedLevel;
        }

        private void onAddDecision(Decision decision, StateMachine<RequestState, Triggers>.Transition transition)
        {
            Decisions.Add(decision);
        }

        private void notifyRequester(StateMachine<RequestState, Triggers>.Transition transition)
        {
            Console.WriteLine();
            Console.WriteLine("======== Notification to Requester ========");
            Console.WriteLine($"Hi {Requester.Name}, Your request state has been changed from {transition.Source} to {transition.Destination} after {transition.Trigger}");
            Console.WriteLine("===========================================");
            Console.WriteLine();
        }

        private void modificationNeeded(Decision decision, StateMachine<RequestState, Triggers>.Transition transition)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("======== Modification Notification ========");
            Console.WriteLine($"Hi {Requester.Name}, Your request need modification after {transition.Trigger}");
            Console.WriteLine("===========================================");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("======== Update Request ========");
            var requester = Helpers.GetPerson();
            Requester = requester;
            Post();
            Console.WriteLine();
        }

        #endregion

        public static class States
        {
            public static RequestState AwaitGroupManagerDecision => new RequestState {Result = RequestState.ResultType.InProgress, SubState = "AwaitGroupManagerDecision"};
            public static RequestState AwaitDepartmentManagerDecision => new RequestState {Result = RequestState.ResultType.InProgress, SubState = "AwaitDepartmentManagerDecision"};
            public static RequestState NeedModification => new RequestState {Result = RequestState.ResultType.InProgress, SubState = "NeedModification"};            
        }
    }
}