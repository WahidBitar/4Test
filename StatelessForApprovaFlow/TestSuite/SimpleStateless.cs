using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;
using NUnit.Framework;


namespace TestSuite
{
    [TestFixture]
    public class SimpleStateless
    {
        private string getDataTest = "";
        
        /// <summary>
        /// Should be able to configure a state machine and fire triggers 
        /// to shift the machine's state       
        /// </summary>
        [Test]
        public void CanBuildStateMachineAndAdvanceStates()
        {
            string startState = "RequestPromotionForm";

            var statemachine = new StateMachine<string, string>(startState);

            //  Request Promo form states
            statemachine.Configure("RequestPromotionForm")
                                    .Permit("Complete", "ManagerReview");

            //  Manager Review states
            statemachine.Configure("ManagerReview")
                                    .Permit("RequestInfo", "RequestPromotionForm")
                                    .Permit("Deny", "PromotionDenied")
                                    .Permit("Approve", "VicePresidentApprove");

            //  Vice President state configuration
            statemachine.Configure("VicePresidentApprove")
                                    .Permit("ManagerJustify", "ManagerReview")
                                    .Permit("Deny", "PromotionDenied")
                                    .Permit("Approve", "Promoted");

            //  Tests            
            Assert.AreEqual(startState, statemachine.State);

            //  Move to next state
            statemachine.Fire("Complete");
            Assert.IsTrue(statemachine.IsInState("ManagerReview"));

            statemachine.Fire("Deny");
            Assert.IsTrue(statemachine.IsInState("PromotionDenied"));
        }
                
        public void GetData()
        {
            this.getDataTest = "we did it";
        }
    }
}
