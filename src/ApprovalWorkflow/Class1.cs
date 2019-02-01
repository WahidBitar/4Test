using System;
using System.Collections.Generic;

namespace ApprovalWorkflow
{
    public struct Trigger
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public struct State
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    /*public struct StateConfig
    {
        public string SourceState { get; set; }
        public string TargetState { get; set; }
        public string Trigger { get; set; }
        public string Description { get; set; }
    }

    public class WorkflowDefinition
    {
        public string WorkflowType { get; set; }
        public IList<State> States { get; set; } = new List<State>();
        public IList<Trigger> Triggers { get; set; } = new List<Trigger>();
        public IList<StateConfig> StateConfigs { get; set; } = new List<StateConfig>();
    }*/

    public abstract class BaseRequest
    {
    }
}
