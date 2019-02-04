using System;
using Core;
using Core.ApprovalAbstraction;

namespace ConsoleApp
{
    class Program
    {
        //private static SimpleRequest request;
        private static AnotherRequest request;

        static void Main(string[] args)
        {
            var requester = Helpers.GetPerson("======= New Request =======",true);

            //request = new SimpleRequest(1, requester);
            request = new AnotherRequest(RequestState.Created, requester);
            request.Post();

            //while (request.CurrentState<6)
            while (request.CurrentState.Result == RequestState.ResultType.InProgress)
            {
                decision();
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"Request Decisions:");
            foreach (var decision in request.Decisions)
            {
                Console.WriteLine($"   * {decision.Result} by {decision.Approver.Name} as a {decision.Approver.Level} who is working in {decision.Approver.WorkPlace}");
            }
            Console.ReadLine();
        }

        private static void decision()
        {
            var approver = Helpers.GetPerson("=====***==== Decision ====***====",false);
            Console.WriteLine("Please add your decision");
            Console.WriteLine("Approved = 1, Rejected = 2, AskForModification = 3");
            var decisionResult = (Decision.DecisionResults) int.Parse(Console.ReadLine());
            var decision = new Decision()
            {
                Approver = approver,
                Result = decisionResult,
            };
            request.AddDecision(decision);
        }


    }
}
