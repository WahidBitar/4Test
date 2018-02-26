using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement
{
    public static class StringHelpers
    {
        private static Random random = new Random();

        public static string UpdateNotification(string oldNotification, string notification)
        {
            return string.IsNullOrEmpty(oldNotification)
                ? notification
                : string.Join(",", oldNotification.Split(',').Append(notification));
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
