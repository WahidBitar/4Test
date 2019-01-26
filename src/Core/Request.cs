using System;
using Stateless;

namespace Core
{
    public class Request
    {
        public enum RequestState
        {
            Created,
            Inprogress,
            GroupManagerReview,
            DivisionManagerReview,
            DepartmentManagerReview,
            Approved,
            Rejected
        }

        public enum Trigger
        {
            Post,
            Approve,
            Reject,
            AskForJustify
        }


        private StateMachine<RequestState, Trigger> machine;
        public int Id { get; set; }
        public string Text { get; set; }
        public RequestState State { get; set; }

        public void ConfigureMachine()
        {
            machine = new StateMachine<RequestState, Trigger>(State);

            machine.Configure(RequestState.Created)
                .Permit(Trigger.Post, RequestState.Inprogress)
                .OnEntry(requestPostedNotification)
                .OnEntry(notifyApprover);

            machine.Configure(RequestState.GroupManagerReview)
                .SubstateOf(RequestState.Inprogress)
                .Permit(Trigger.Approve, groupUpLevel())
                .OnEntry(requestPostedNotification);
        }

        private RequestState groupUpLevel()
        {
            return RequestState.DivisionManagerReview;
        }

        private void requestPostedNotification()
        {
            Console.WriteLine($"New request containing the following text '{Text}' has been Posted");
        }

        private void notifyApprover(StateMachine<RequestState, Trigger>.Transition actionTransition)
        {
            Console.WriteLine($"New request awaiting your decision");
        }
    }


}