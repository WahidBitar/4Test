using System;
using Core;

namespace ConsoleApp
{
    class Program
    {
        private static SimpleRequest request;

        static void Main(string[] args)
        {
            var requester = Helpers.GetPerson();
            Console.WriteLine();

            request = new SimpleRequest(1, requester);
            request.Post();
            Console.WriteLine();
            while (request.CurrentState < 6)
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
            Console.WriteLine("=====***==== Decision ====***====");
            var approver = Helpers.GetPerson();
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
