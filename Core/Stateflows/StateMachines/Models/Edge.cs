﻿using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.Common;

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
        private string triggerDescriptor = null;
        private string TriggerDescriptor
            => triggerDescriptor ??= IsElse
                ? $"{Trigger}|else"
                : Trigger;

        public Type TriggerType { get; set; }

        public IEnumerable<Type> ActualTriggerTypes { get; set; }

        public IEnumerable<Type> TimeTriggerTypes { get; set; }

        public string Trigger { get; set; }

        public IEnumerable<string> ActualTriggers { get; set; }

        public EdgeType Type { get; set; }
        public bool IsElse { get; set; }

        private Logic<StateMachinePredicateAsync> guards = null;
        public Logic<StateMachinePredicateAsync> Guards => guards ??= new Logic<StateMachinePredicateAsync>() { Name = Constants.Guard };

        private Logic<StateMachineActionAsync> effects = null;
        public Logic<StateMachineActionAsync> Effects => effects ??= new Logic<StateMachineActionAsync>() { Name = Constants.Effect };

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
            var actualTriggerTypes = Graph.GetTriggerTypes(TriggerType).ToHashSet();
            var result = new Edge()
            {
                Graph = Graph,
                Trigger = Trigger,
                TriggerType = TriggerType,
                ActualTriggerTypes = actualTriggerTypes,
                TimeTriggerTypes = actualTriggerTypes.Where(type => type.IsSubclassOf(typeof(TimeEvent))).ToHashSet(),
                ActualTriggers = actualTriggerTypes.Select(type => Event.GetName(type)).ToHashSet(),
                Type = Type,
                IsElse = edge.IsElse,
                SourceName = SourceName,
                Source = Source,
                TargetName = edge.TargetName,
                Target = edge.Target
            };

            result.Guards.Actions.AddRange(Guards.Actions);
            result.Guards.Actions.AddRange(edge.Guards.Actions);

            result.Effects.Actions.AddRange(Effects.Actions);
            result.Effects.Actions.AddRange(edge.Effects.Actions);

            return result;
        }
    }
}
