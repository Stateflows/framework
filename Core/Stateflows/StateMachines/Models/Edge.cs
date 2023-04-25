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

        public Logic<StateMachinePredicateAsync> guards = null;
        public Logic<StateMachinePredicateAsync> Guards => guards ?? (guards = new Logic<StateMachinePredicateAsync>() { Name = Constants.Guard, Graph = Graph });

        public Logic<StateMachineActionAsync> effects = null;
        public Logic<StateMachineActionAsync> Effects => effects ?? (effects = new Logic<StateMachineActionAsync>() { Name = Constants.Effect, Graph = Graph });

        public string SourceName { get; set; }
        public Vertex Source { get; set; }
        public string TargetName { get; set; }
        public Vertex Target { get; set; }
    }
}
