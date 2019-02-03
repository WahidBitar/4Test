using System.Collections.Generic;
using Stateless;

namespace Core
{
    public class SimpleRequest
    {
        private readonly StateMachine<States, Triggers>.TriggerWithParameters<Decision> approveTrigger;
        private readonly StateMachine<States, Triggers>.TriggerWithParameters<Decision> askForModificationTrigger;
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
            askForModificationTrigger = stateMachine.SetTriggerParameters<Decision>(Triggers.AskForModification);

            stateMachine.Configure(States.Created)
                .PermitIf(Triggers.Post,
                    States.AwaitGroupManagerDecision,
                    () => Requester.WorkPlace == Person.WorkPlaces.Group)
                .PermitIf(Triggers.Post,
                    States.AwaitDivisionManagerDecision,
                    () => Requester.WorkPlace == Person.WorkPlaces.Division)
                .PermitIf(Triggers.Post,
                    States.AwaitDepartmentManagerDecision,
                    () => Requester.WorkPlace == Person.WorkPlaces.Department);

            stateMachine.Configure(States.NeedModification)
                .SubstateOf(States.Created)
                .OnEntryFrom(askForModificationTrigger, onAddDecision)
                .OnEntryFrom(askForModificationTrigger, modificationNeeded);

            stateMachine.Configure(States.AwaitGroupManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.GroupManager))
                .OnEntryFrom(approveTrigger, onAddDecision)
                .OnEntryFrom(rejectTrigger, onAddDecision)
                .PermitIf(askForModificationTrigger, States.NeedModification, decision => canDecide(decision, Person.UserLevels.GroupManager))
                .PermitIf(approveTrigger, States.AwaitDivisionManagerDecision, decision => canDecide(decision, Person.UserLevels.GroupManager))
                .PermitIf(rejectTrigger, States.Rejected, decision => canDecide(decision, Person.UserLevels.GroupManager));

            stateMachine.Configure(States.AwaitDivisionManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.DivisionManager))
                .OnEntryFrom(approveTrigger, onAddDecision)
                .OnEntryFrom(rejectTrigger, onAddDecision)
                .PermitIf(askForModificationTrigger, States.NeedModification, decision => canDecide(decision, Person.UserLevels.DivisionManager))
                .PermitIf(approveTrigger, States.AwaitDepartmentManagerDecision, decision => canDecide(decision, Person.UserLevels.DivisionManager))
                .PermitIf(rejectTrigger, States.Rejected, decision => canDecide(decision, Person.UserLevels.DivisionManager));

            stateMachine.Configure(States.AwaitDepartmentManagerDecision)
                .OnEntry(() => onPost(Person.UserLevels.DepartmentManager))
                .OnEntryFrom(approveTrigger, onAddDecision)
                .OnEntryFrom(rejectTrigger, onAddDecision)
                .PermitIf(askForModificationTrigger, States.NeedModification, decision => canDecide(decision, Person.UserLevels.DepartmentManager))
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
        public Person Requester { get; set; }
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

            if (decision.Result == Decision.DecisionResults.AskForModification)
                stateMachine.Fire(askForModificationTrigger, decision);
        }

        #region Private Methods

        private void modificationNeeded(Decision decision, StateMachine<States, Triggers>.Transition transition)
        {
            Helpers.ModificationNotification(transition.Trigger, Requester.Name);
            var requester = Helpers.GetPerson("======== Update Request ========", true);
            Requester = requester;
            Post();
        }

        private void onReject(StateMachine<States, Triggers>.Transition transition)
        {
            Helpers.OnRejectNotification(Requester.Name, Requester.WorkPlace);
        }

        private void onApprove(StateMachine<States, Triggers>.Transition transition)
        {
            Helpers.OnApproveNotification(Requester.Name, Requester.WorkPlace);
        }

        private void notifyRequester(StateMachine<States, Triggers>.Transition transition)
        {
            Helpers.NotifyRequester(Requester.Name, transition.Source, transition.Destination, transition.Trigger);
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
            Helpers.NotifyApprover(approverLevel);
        }

        private void stateExceptionHandler(States state, Triggers trigger, ICollection<string> args)
        {
            Helpers.HandleException(state, trigger, args);
        }

        #endregion


        private enum Triggers
        {
            Post = 1,
            Approve = 2,
            Reject = 3,
            AskForModification = 4
        }

        private enum States
        {
            Created = 1,
            NeedModification = 2,
            AwaitGroupManagerDecision = 3,
            AwaitDivisionManagerDecision = 4,
            AwaitDepartmentManagerDecision = 5,
            Approved = 6,
            Rejected = 7
        }
    }
}