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
        private string name = null;
        public string Name
            => name ??= $"{SourceName}-{TriggerDescriptor}->{TargetName}";

        private string identifier = null;
        public string Identifier
            => identifier ??= Target != null
                ? $"{Source.Identifier}-{TriggerDescriptor}->{Target.Identifier}"
                : $"{Source.Identifier}-{TriggerDescriptor}";

        private string signature = null;
        public string Signature
            => signature ??= $"{Source.Identifier}-{Trigger}->";

        public Graph Graph { get; set; }
        public string Trigger { get; set; }
        private string triggerDescriptor = null;
        private string TriggerDescriptor
            => triggerDescriptor ??= IsElse
                ? $"{Trigger}|else"
                : Trigger;

        public Type TriggerType { get; set; }
        public TriggerType Type { get; set; }
        public bool IsElse { get; set; }

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
