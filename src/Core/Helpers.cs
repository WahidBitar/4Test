using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public static class Helpers
    {
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