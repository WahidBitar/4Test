using System;
using Core;

namespace ConsoleApp
{
    class Program
    {
        private static SimpleRequest request;

        static void Main(string[] args)
        {
            Console.WriteLine("Please enter your name");
            var name = Console.ReadLine();
            Console.WriteLine("Please enter your level");
            var level = (Person.UserLevels) int.Parse(Console.ReadLine());
            var requester = new Person()
            {
                Name = name,
                Level = level,
            };
            request = new SimpleRequest(1, requester);
            request.Post();
            Console.WriteLine();
            while (request.CurrentState < 5)
            {
                decision();
            }
            Console.ReadLine();
        }

        private static void decision()
        {
            Console.WriteLine("=====***===* Decision *===***====");
            Console.WriteLine("Please enter your name");
            var name = Console.ReadLine();
            Console.WriteLine("Please enter your level");
            var level = (Person.UserLevels) int.Parse(Console.ReadLine());
            var approver = new Person()
            {
                Name = name,
                Level = level,
            };
            Console.WriteLine("Please add the decision Id.");
            var decisionId = int.Parse(Console.ReadLine());
            Console.WriteLine("Please add your decision. 1 for approve 2 for reject and 3 for modification");
            var decisionResult = (Decision.DecisionResults) int.Parse(Console.ReadLine());
            var decision = new Decision()
            {
                Approver = approver,
                Result = decisionResult,
                Id = decisionId,
            };
            request.AddDecision(decision);
        }
    }
}
