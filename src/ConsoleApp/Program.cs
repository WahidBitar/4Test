using System;
using Core;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter your request text");
            var request = new Request
            {
                Id = 1,
                State = Request.RequestState.Created,
                Text = Console.ReadLine(),
            };
            request.ConfigureMachine();
            request.Post();
            //request.Reject(new Approver() {JobTitle = JobTitles.GroupManager}, "Group manager approved");
            request.Approve(new Approver() {JobTitle = JobTitles.GroupManager}, "Group manager approved");
            Console.ReadLine();
        }
    }
}
