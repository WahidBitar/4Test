using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public static class Helpers
    {
        public static Person GetPerson(string reason, bool isRequester)
        {
            if (!string.IsNullOrEmpty(reason))
                Console.WriteLine(reason);

            Console.WriteLine();
            Console.WriteLine("Please enter your name");
            var name = Console.ReadLine();
            var level = Person.UserLevels.Employee;
            var workPlace = Person.WorkPlaces.Group;
            if (!isRequester)
            {
                Console.WriteLine("Please enter your level");
                Console.WriteLine($"Employee = 0, GroupManager = 1, DivisionManager = 2, DepartmentManager = 3");
                level = (Person.UserLevels) int.Parse(Console.ReadLine());
            }
            else
            {
                Console.WriteLine("Please enter your Work Place");
                Console.WriteLine($"Group = 1, Division = 2, Department = 3");
                workPlace = (Person.WorkPlaces) int.Parse(Console.ReadLine());
            }

            var requester = new Person()
            {
                Name = name,
                Level = level,
                WorkPlace = workPlace,
            };
            return requester;
        }


        public static void NotifyRequester(object requesterName, object sourceState, object destination, object trigger)
        {
            Console.WriteLine();
            Console.WriteLine("======== Notification to Requester ========");
            Console.WriteLine($"Hi {requesterName}, Your request state has been changed from {sourceState} to {destination} after {trigger}");
            Console.WriteLine("===========================================");
            Console.WriteLine();
        }
        public static void NotifyApprover(object level)
        {
            Console.WriteLine();
            Console.WriteLine("======== Notification to Approver =========");
            Console.WriteLine($"Hi, as you are {level} there is a new request awaiting your decision");
            Console.WriteLine("===========================================");
            Console.WriteLine();
        }

        public static void ModificationNotification(object trigger, object requesterName)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("======== Modification Notification ========");
            Console.WriteLine($"Hi {requesterName}, Your request need modification after {trigger}");
            Console.WriteLine("===========================================");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void OnRejectNotification(object requesterName, object workPlace)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("=================================");
            Console.WriteLine($"The request of {requesterName} from {workPlace} has been Rejected");
            Console.WriteLine("=================================");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        public static void OnApproveNotification(object requesterName, object workPlace)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=================================");
            Console.WriteLine($"The request of {requesterName} from {workPlace} has been Approved");
            Console.WriteLine("=================================");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        public static void HandleException(object state, object trigger, ICollection<string> args)
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

        private static readonly object locker = new object();
        private static readonly List<int> randoms = new List<int>();

        static Helpers()
        {
            for (int i = 1; i <= 100; i++)
            {
                randoms.Add(i);
            }
        }

        public static int GetNumber()
        {
            lock (locker)
            {
                if (randoms.Any())
                {
                    var x = randoms[0];
                    randoms.RemoveAt(0);
                    return x;
                }
            }

            return 0;
        }
    }
}