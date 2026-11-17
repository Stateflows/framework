using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        internal string? SourcePath { get; set; }

        public string OriginStateMachineName { get; set; } = null;
        
        public string Name { get; set; }

        public string Identifier { get; set; }
        internal Type? ImplementationType { get; set; }

        public string Signature { get; set; }

        public Graph Graph { get; set; }

        public Type TriggerType { get; set; }
        public IReadOnlyList<string>? PayloadNames { get; set; }

        private bool polymorphicTriggers = false;
        private bool polymorphicTriggersSet = false;

        public bool PolymorphicTriggers
        {
            get => polymorphicTriggersSet
                ? polymorphicTriggers
                : Graph.PolymorphicTriggers;

            set
            {
                polymorphicTriggers = value;
                polymorphicTriggersSet = true;
            }
        }

        public IEnumerable<Type> ActualTriggerTypes { get; set; }

        public IEnumerable<Type> TimeTriggerTypes { get; set; }

        public IEnumerable<Type> RecurringTypes { get; set; }

        public string Trigger { get; set; }

        public IEnumerable<string> ActualTriggers { get; set; }

        public EdgeType Type { get; set; }
        
        public bool IsElse { get; set; }

        public bool IsLocal { get; set; } = true;


        public Logic<StateMachinePredicateAsync> Guards { get; } = new(Constants.Guard);

        public Logic<StateMachineActionAsync> Effects { get; } = new(Constants.Effect);

        public string SourceName { get; set; }
        public Vertex Source { get; set; }
        public string TargetName { get; set; }
        public Vertex Target { get; set; }

        public Dictionary<string, object> Metadata { get; } = [];

        public IEnumerable<TransitionRoute> GetRoutes()
            => Target?.Type == VertexType.Junction
                ? Target.OrderedEdges.SelectMany(edge => edge.GetRoutes())
                    .Select(route => new TransitionRoute(Array.AsReadOnly(new[] { this }.Concat(route.Segments).ToArray())))
                : new[] { new TransitionRoute(Array.AsReadOnly(new[] { this })) };
    }
}
