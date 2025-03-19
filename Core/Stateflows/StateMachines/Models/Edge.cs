using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Models
{
    internal enum EdgeType
    {
        Transition,
        InternalTransition,
        DefaultTransition
    }

    internal class Edge
    {
        public string OriginStateMachineName { get; set; } = null;
        
        public string Name { get; set; }

        public string Identifier { get; set; }

        public string Signature { get; set; }

        public Graph Graph { get; set; }

        public Type TriggerType { get; set; }

        public IEnumerable<Type> ActualTriggerTypes { get; set; }

        public IEnumerable<Type> TimeTriggerTypes { get; set; }

        public IEnumerable<Type> RecurringTypes { get; set; }

        public string Trigger { get; set; }

        public IEnumerable<string> ActualTriggers { get; set; }

        public EdgeType Type { get; set; }
        
        public bool IsElse { get; set; }

        public bool IsLocal { get; set; } = true;

        public Logic<StateMachinePredicateAsync> Guards { get; } = new Logic<StateMachinePredicateAsync>(Constants.Guard);

        public Logic<StateMachineActionAsync> Effects { get; } = new Logic<StateMachineActionAsync>(Constants.Effect);

        public string SourceName { get; set; }
        public Vertex Source { get; set; }
        public string TargetName { get; set; }
        public Vertex Target { get; set; }

        public IEnumerable<Edge> GetActualEdges()
            => Target?.Type == VertexType.Junction
                ? Target.Edges.Values.Select(edge => MergeWith(edge)).ToArray()
                : new Edge[] { this };

        private Edge MergeWith(Edge edge)
        {
            var actualTriggerTypes = Graph.StateflowsBuilder.TypeMapper.GetMappedTypes(TriggerType).ToHashSet();

            var triggerDescriptor = edge.IsElse
                ? $"{Trigger}|else"
                : Trigger;

            var identifier = edge.Target != null
                ? $"{Source.Identifier}-{triggerDescriptor}->{edge.Target.Identifier}"
                : $"{Source.Identifier}-{triggerDescriptor}";

            var result = new Edge()
            {
                Graph = Graph,
                Trigger = Trigger,
                TriggerType = TriggerType,
                ActualTriggerTypes = actualTriggerTypes,
                TimeTriggerTypes = actualTriggerTypes.Where(type => type.IsSubclassOf(typeof(TimeEvent))).ToHashSet(),
                RecurringTypes = actualTriggerTypes.Where(type => type.IsSubclassOf(typeof(RecurringEvent))).ToHashSet(),
                ActualTriggers = actualTriggerTypes.Select(type => Event.GetName(type)).ToHashSet(),
                Type = Type,
                IsElse = edge.IsElse,
                SourceName = SourceName,
                Source = Source,
                TargetName = edge.TargetName,
                Target = edge.Target,
                Name = $"{SourceName}-{triggerDescriptor}->{edge.TargetName}",
                Identifier = identifier,
                Signature = $"{Source.Identifier}-{Trigger}->",
            };

            result.Guards.Actions.AddRange(Guards.Actions);
            result.Guards.Actions.AddRange(edge.Guards.Actions);

            result.Effects.Actions.AddRange(Effects.Actions);
            result.Effects.Actions.AddRange(edge.Effects.Actions);

            return result;
        }
    }
}
