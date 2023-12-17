using System;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Models
{
    internal enum TriggerType
    {
        Transition,
        InternalTransition,
        DefaultTransition
    }

    internal class Edge
    {
        public Graph Graph { get; set; }
        public string Trigger { get; set; }
        public Type TriggerType { get; set; }
        public TriggerType Type { get; set; }
        public bool IsElseTransition { get; set; }
        public string Name
            => $"{SourceName}:{Trigger}:{TargetName}";
        public string Identifier
            => Target != null
                ? $"{Source.Identifier}:{Trigger}:{Target.Identifier}"
                : $"{Source.Identifier}:{Trigger}";

        public Logic<StateMachinePredicateAsync> guards = null;
        public Logic<StateMachinePredicateAsync> Guards => guards ??= new Logic<StateMachinePredicateAsync>() { Name = Constants.Guard };

        public Logic<StateMachineActionAsync> effects = null;
        public Logic<StateMachineActionAsync> Effects => effects ??= new Logic<StateMachineActionAsync>() { Name = Constants.Effect };

        public string SourceName { get; set; }
        public Vertex Source { get; set; }
        public string TargetName { get; set; }
        public Vertex Target { get; set; }
    }
}
