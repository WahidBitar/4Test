using System;
using System.Collections;
using System.Collections.Generic;
using Stateless;

namespace Core
{
    public class Request
    {
        public enum RequestState
        {
            Created,
            GroupManagerReview,
            DivisionManagerReview,
            DepartmentManagerReview,
            Approved,
            Rejected,
            ModificationNeeded
        }

        private enum Trigger
        {
            Post,
            Approve,
            Reject,
            AskForJustify
        }


        private StateMachine<RequestState, Trigger> machine;

        public int Id { get; set; }
        public string Text { get; set; }
        public WorkPlace WorkPlace { get; set; }
        public RequestState State { get; set; }
        public IList<Decision> Decisions { get; set; } = new List<Decision>();

        public void ConfigureMachine()
        {
            machine = new StateMachine<RequestState, Trigger>(State);

            machine.Configure(RequestState.Created)
                .PermitDynamic(Trigger.Post, setTargetByWorkplace);


            machine.Configure(RequestState.GroupManagerReview)
                .PermitDynamic(Trigger.Approve, setTargetByHierarchy)
                .Permit(Trigger.Reject, RequestState.Rejected)
                .Permit(Trigger.AskForJustify, RequestState.ModificationNeeded)
                .OnEntry(notifyApprover)
                .OnEntry(requestProgressNotification);


            machine.Configure(RequestState.DivisionManagerReview)
                .PermitDynamic(Trigger.Approve, setTargetByHierarchy)
                .Permit(Trigger.Reject, RequestState.Rejected)
                .Permit(Trigger.AskForJustify, RequestState.ModificationNeeded)
                .OnEntry(notifyApprover)
                .OnEntry(requestProgressNotification);


            machine.Configure(RequestState.DepartmentManagerReview)
                .Permit(Trigger.Approve, RequestState.Approved)
                .Permit(Trigger.Reject, RequestState.Rejected)
                .Permit(Trigger.AskForJustify, RequestState.ModificationNeeded)
                .OnEntry(notifyApprover)
                .OnEntry(requestProgressNotification);
        }

        public void Post()
        {
            machine.Fire(Trigger.Post);
        }

        private RequestState setTargetByHierarchy()
        {
            return RequestState.DivisionManagerReview;
        }

        private RequestState setTargetByWorkplace()
        {
            switch (WorkPlace)
            {
                case WorkPlace.Group:
                    return RequestState.GroupManagerReview;
                case WorkPlace.Division:
                    return RequestState.DivisionManagerReview;
                case WorkPlace.Department:
                    return RequestState.DepartmentManagerReview;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void notifyApprover(StateMachine<RequestState, Trigger>.Transition actionTransition)
        {
            Console.WriteLine("New request awaiting decision");
        }


        private void requestProgressNotification(StateMachine<RequestState, Trigger>.Transition actionTransition)
        {
            Console.WriteLine($"The request status is changing from {actionTransition.Source} to {actionTransition.Destination}");
        }

        public void Approve(Approver approver, string notes)
        {
            machine.Fire(Trigger.Approve);
            Decisions.Add(new Decision
            {
                Approver = approver,
                Notes = notes,
                Type = Decision.DecisionType.Approved
            });
        }

        public void Reject(Approver approver, string notes)
        {
            machine.Fire(Trigger.Reject);
            Decisions.Add(new Decision
            {
                Approver = approver,
                Notes = notes,
                Type = Decision.DecisionType.Rejected
            });
        }
    }


}