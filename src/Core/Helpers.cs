using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public static class Helpers
    {
        public static Person GetPerson()
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