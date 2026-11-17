using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Models
{
    internal class Vertex
    {
        internal string? SourcePath { get; set; }
        internal Dictionary<string, string> DeferralPaths { get; } = [];

        public Graph Graph { get; set; }
        public Region ParentRegion { get; set; }
        public string Name { get; set; }
        public string? OriginStateMachineName { get; set; }
        public VertexType Type { get; set; }
        public string Identifier => Name;
        internal string DefinitionIdentifier => $"vertex:{Name.Length}:{Name}";
        internal Type? ImplementationType { get; set; }
        internal Type? HostedImplementationType { get; set; }

        private Logic<StateMachineActionAsync> initialize = null;
        public Logic<StateMachineActionAsync> Initialize => initialize ??= new Logic<StateMachineActionAsync>(Constants.Initialize);

        private Logic<StateMachineActionAsync> finalize = null;
        public Logic<StateMachineActionAsync> Finalize => finalize ??= new Logic<StateMachineActionAsync>(Constants.Finalize);

        private Logic<StateMachineActionAsync> entry = null;
        public Logic<StateMachineActionAsync> Entry => entry ??= new Logic<StateMachineActionAsync>(Constants.Entry);

        private Logic<StateMachineActionAsync> exit = null;
        public Logic<StateMachineActionAsync> Exit => exit ??= new Logic<StateMachineActionAsync>(Constants.Exit);

        public Dictionary<string, Edge> Edges { get; set; } = new Dictionary<string, Edge>();
        public IEnumerable<Edge> OrderedEdges => Edges.Values.OrderBy(edge => edge.IsElse);
        public IEnumerable<Edge> IncomingEdges => Graph.AllEdges.Where(edge => edge.Target == this);
        public Dictionary<string, Logic<StateMachinePredicateAsync>> Deferrals { get; set; } = new();
        internal int NextGuardDelegationIndex { get; set; }
        public Dictionary<string, Type> DeferralTypes { get; } = new();
        internal Dictionary<Type, Logic<StateMachinePredicateAsync>> DeferralDeclarations { get; } = [];
        internal bool TryGetInheritedDeferral(Type eventType, out Logic<StateMachinePredicateAsync> logic)
        {
            return DeferralDeclarations.TryGetValue(eventType, out logic) && logic.OriginStateMachineName != null;
        }
        internal void PrepareDeferrals()
        {
            foreach (var name in DeferralTypes.Where(pair => DeferralDeclarations.ContainsKey(pair.Value)).Select(pair => pair.Key).ToArray())
            {
                Deferrals.Remove(name);
                DeferralTypes.Remove(name);
            }
            foreach (var declaration in DeferralDeclarations)
            {
                var names = Graph.StateflowsBuilder.TypeMapper.GetMappedTypes(declaration.Key).Select(Event.GetName).Distinct().ToArray();
                if (names.Length == 0)
                    throw new DeferralDefinitionException(Event.GetName(declaration.Key), "Deferred event must have at least one effective name.", Graph.Class);
                foreach (var name in names)
                {
                    if (!Deferrals.TryAdd(name, declaration.Value))
                        throw new DeferralDefinitionException(name, $"Event '{name}' is already deferred by '{Name}'.", Graph.Class);
                    DeferralTypes.Add(name, declaration.Key);
                }
            }
        }
        public HashSet<string> PayloadDeferrals { get; } = new(StringComparer.Ordinal);
        public List<Region> Regions { get; set; } = [];
        public Region DefaultRegion
        {
            get
            {
                if (!Regions.Any())
                {
                    Regions.Add(new Region()
                    {
                        Graph = Graph,
                        ParentVertex = this,
                        OriginStateMachineName = OriginStateMachineName
                    });
                }

                return Regions.First();
            }
        }
        
        /// <summary>
        /// Refers to a region that current vertex serves as a history entrypoint (History vertices only!)
        /// </summary>
        public Region HistoricalRegion { get; set; }
        public StateActionInitializationBuilder BehaviorInitializationBuilder { get; set; }
        public string BehaviorName { get; set; }
        public string BehaviorType { get; set; }
        public List<Type> BehaviorEventTypes { get; set; } = [];
        public HashSet<string> BehaviorPayloads { get; } = new(StringComparer.Ordinal);
        internal Dictionary<Type, HashSet<string>> BehaviorEventBindings { get; } = [];

        public Dictionary<string, object> Metadata { get; } = [];

        public BehaviorId GetBehaviorId(StateMachineId hostId) => new(BehaviorType, BehaviorName, hostId.Instance);

        public bool IsChildOf(Vertex vertex)
        {
            var currentVertex = this;
            while (currentVertex != null)
            {
                if (currentVertex == vertex)
                {
                    return true;
                }

                currentVertex = currentVertex?.ParentRegion?.ParentVertex;
            }
            
            return false;
        }
        
        public bool IsOrthogonalTo(Vertex vertex)
        {
            if (this == vertex)
            {
                return false;
            }

            var stack = new List<Vertex>();
            var currentVertex = this;
            while (currentVertex != null)
            {
                stack.Add(currentVertex);

                currentVertex = currentVertex?.ParentRegion?.ParentVertex;
            }

            currentVertex = vertex;
            Vertex previousVertex = null;
            while (currentVertex != null)
            {
                var index = stack.IndexOf(currentVertex);
                if (index > 0 && previousVertex != null && stack[index - 1].ParentRegion != previousVertex.ParentRegion)
                {
                    return true;
                }

                previousVertex = currentVertex;
                currentVertex = currentVertex?.ParentRegion?.ParentVertex;
            }

            return false;
        }

        public IEnumerable<Vertex> GetBranch()
        {
            var result = new List<Vertex> { this };
            if (Regions.Any())
            {
                result.AddRange(Regions.SelectMany(region =>
                    region.Vertices.Values.SelectMany(vertex => vertex.GetBranch())));
            }

            return result;
        }

        public IEnumerable<Region> GetParentRegions()
        {
            var result = new List<Region>();
            var currentRegion = ParentRegion;
            while (currentRegion != null)
            {
                result.Add(currentRegion);

                currentRegion = currentRegion.ParentVertex.ParentRegion;
            }

            return result;
        }
    }
}
