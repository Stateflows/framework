using System;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Models
{
    internal class Edge
    {
        public Graph Graph { get; set; }
        public string Trigger { get; set; }
        public Type TriggerType { get; set; }

        public Action<StateMachinePredicateAsync> guards = null;
        public Action<StateMachinePredicateAsync> Guards => guards ?? (guards = new Action<StateMachinePredicateAsync>() { Name = Constants.Guard, Graph = Graph });

        public Action<StateMachineActionAsync> effects = null;
        public Action<StateMachineActionAsync> Effects => effects ?? (effects = new Action<StateMachineActionAsync>() { Name = Constants.Effect, Graph = Graph });

        public string SourceName { get; set; }
        public Vertex Source { get; set; }
        public string TargetName { get; set; }
        public Vertex Target { get; set; }
    }
}
