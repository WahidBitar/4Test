using System;
using Core;

namespace ConsoleApp
{
    class Program
    {
        private static SimpleRequest request;

        static void Main(string[] args)
        {
            var requester = GetPerson();
            Console.WriteLine();

            request = new SimpleRequest(1, requester);
            request.Post();
            Console.WriteLine();
            while (request.CurrentState < 5)
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
            var approver = GetPerson();
            Console.WriteLine("Please add your decision. 1 for approve 2 for reject and 3 for modification");
            var decisionResult = (Decision.DecisionResults) int.Parse(Console.ReadLine());
            var decision = new Decision()
            {
                Approver = approver,
                Result = decisionResult,
            };
            request.AddDecision(decision);
        }

        private static Person GetPerson()
        {
            Console.WriteLine("Please enter your name");
            var name = Console.ReadLine();
            Console.WriteLine("Please enter your level");
            Console.WriteLine($"Employee = 0, GroupManager = 1, DivisionManager = 2, DepartmentManager = 3");
            var level = (Person.UserLevels) int.Parse(Console.ReadLine());
            Console.WriteLine("Please enter your Work Place");
            Console.WriteLine($"Group = 1, Division = 2, Department = 3");
            var workPlace = (Person.WorkPlaces) int.Parse(Console.ReadLine());
            var requester = new Person()
            {
                Name = name,
                Level = level,
                WorkPlace = workPlace,
            };
            return requester;
        }
    }
}
