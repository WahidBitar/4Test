using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using StatelessForApprovaFlow;

namespace TestSuite
{
    [TestFixture]
    public class RequestPromotionTests
    {
        /// <summary>
        /// Should be able to read a json stream, deserialize States, Triggers
        /// and configure a RequestPromotion workflow.
        /// 
        /// NOTE:  Change source to match your project location
        /// </summary>
        
        [Test]
        public void CanConfigureWorkflowWithJSON()
        {
            string source = @"F:\vs10dev\StatelessForApprovaFlow\TestSuite\TestData\RequestPromotion.json";
            string startState = "RequestPromotionForm";

            var requestPromotion = new RequestPromotion(source, startState);
            requestPromotion.Configure();

            Assert.AreEqual(startState, requestPromotion.GetCurrentState());
        }

        /// <summary>
        /// Should be able to change the state of RequestionPromotion
        /// from RequestPromotionForm to ManagerReview to VicePresidentApprove to
        ///    ManagerReview To PromotionDenied
        /// </summary>
        [Test]
        public void CanProgressRequestPromotionStateMachine()
        {
            string source = @"F:\vs10dev\StatelessForApprovaFlow\TestSuite\TestData\RequestPromotion.json";
            string startState = "RequestPromotionForm";

            var requestPromotion = new RequestPromotion(source, startState);
            requestPromotion.Configure();

            //  RequestPromotionForm to ManagerReview
            requestPromotion.ProgressToNextState("Complete");
            Assert.AreEqual("ManagerReview", requestPromotion.GetCurrentState());

            //  ManagerReview to VicePresidentApprove
            requestPromotion.ProgressToNextState("Approve");
            Assert.AreEqual("VicePresidentApprove", requestPromotion.GetCurrentState());

            //  VP To ManagerReview
            requestPromotion.ProgressToNextState("ManagerJustify");
            Assert.AreEqual("ManagerReview", requestPromotion.GetCurrentState());

            //  Deny it
            requestPromotion.ProgressToNextState("Deny");
            Assert.AreEqual("PromotionDenied", requestPromotion.GetCurrentState());
        }

        /// <summary>
        /// Should through ApplicationException for an incorrect
        /// trigger
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public void CanThrowExceptionForImproperTrigger()
        {
            string source = @"F:\vs10dev\StatelessForApprovaFlow\TestSuite\TestData\RequestPromotion.json";
            string startState = "RequestPromotionForm";

            var requestPromotion = new RequestPromotion(source, startState);
            requestPromotion.Configure();
            requestPromotion.ProgressToNextState("Deny");            
        }
    }
}
