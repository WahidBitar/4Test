using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;
using StatelessForApprovaFlow.Utils;
using System.IO;
using Newtonsoft.Json;

namespace StatelessForApprovaFlow
{
    public class RequestPromotion
    {
        private StateMachine<string, string> stateMachine;
        private string source;
        private string startState;

        public RequestPromotion(string source, string startState)
        {
            this.source = source;
            this.startState = startState;
        }

        /// <summary>
        /// Given a json stream, configure the states, triggers and progression
        /// paths based on State => Trigger => TargetState directives
        /// </summary>
        public void Configure() 
        {
            Enforce.That((string.IsNullOrEmpty(source) == false),
                            "RequestPromotion.Configure - source is null");
            
            string json = GetJson(source);

            var workflowDefintion = JsonConvert.DeserializeObject<WorkflowDefinition>(json);

            Enforce.That((string.IsNullOrEmpty(startState) == false),
                            "RequestPromotion.Configure - startStep is null");

            this.stateMachine = new StateMachine<string, string>(startState);

            //  Get a distinct list of states with a trigger from state configuration
            //  "State => Trigger => TargetState
            var states = workflowDefintion.StateConfigs.AsQueryable()
                                    .Select(x => x.State)
                                    .Distinct()
                                    .Select(x => x)
                                    .ToList();

            //  Assing triggers to states
            states.ForEach(state =>
            {
                var triggers = workflowDefintion.StateConfigs.AsQueryable()
                                   .Where(config => config.State == state)
                                   .Select(config => new { Trigger = config.Trigger, TargeState = config.TargetState })
                                   .ToList();

                triggers.ForEach(trig =>
                {
                    this.stateMachine.Configure(state).Permit(trig.Trigger, trig.TargeState);
                });
            });
        }

        /// <summary>
        /// Given a trigger, progress the state machine to the target state
        /// </summary>
        /// <param name="trigger">trigger as string</param>
        public void ProgressToNextState(string trigger)
        { 
            Enforce.That((string.IsNullOrEmpty(trigger) == false), 
                            "RequestPromotion.ProgressToNextState - trigger is null");

            this.stateMachine.Fire(trigger);
        }

        /// <summary>
        /// What is current state?
        /// </summary>
        /// <returns>The current state as stringS</returns>
        public string GetCurrentState()
        {
            return this.stateMachine.State;
        }

        /// <summary>
        /// Fetch json stream from disk
        /// </summary>
        /// <param name="source">Path to source as string</param>
        /// <returns>File contents as string</returns>
        private string GetJson(string source)
        {
            var fileInfo = new FileInfo(source);

            if (fileInfo.Exists == false)
            {
                throw new ApplicationException("RequestPromotion.Configure - File not found");
            }

            StreamReader sr = fileInfo.OpenText();
            string json = sr.ReadToEnd();
            sr.Close();

            return json;
        }
    }
}
