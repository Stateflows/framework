using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Models
{
    internal class Vertex
    {
        public Graph Graph { get; set; }
        public Region ParentRegion { get; set; }
        public string Name { get; set; }
        public string OriginStateMachineName { get; set; } = null;
        public VertexType Type { get; set; }
        public string Identifier => Name;

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

        public List<string> DeferredEvents { get; set; } = new List<string>();
        public List<Region> Regions { get; set; } = new List<Region>();
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

        public StateActionInitializationBuilder BehaviorInitializationBuilder { get; set; }
        public string BehaviorName { get; set; }
        public string BehaviorType { get; set; }
        public List<Type> BehaviorSubscriptions { get; set; } = new List<Type>();
        public List<string> GetBehaviorSubscriptionNames()
            => BehaviorSubscriptions
            .Select(t => Event.GetName(t))
            .ToList();

        public Subscribe GetSubscriptionRequest(StateMachineId hostId)
            => new Subscribe()
            {
                BehaviorId = hostId,
                NotificationNames = GetBehaviorSubscriptionNames()
            };

        public Unsubscribe GetUnsubscriptionRequest(StateMachineId hostId)
            => new Unsubscribe()
            {
                BehaviorId = hostId,
                NotificationNames = GetBehaviorSubscriptionNames()
            };
        public List<Type> BehaviorRelays { get; set; } = new List<Type>();
        public List<string> GetBehaviorRelayNames()
            => BehaviorRelays
                .Select(t => Event.GetName(t))
                .ToList();

        public StartRelay GetStartRelayRequest(StateMachineId hostId)
            => new StartRelay()
            {
                BehaviorId = hostId,
                NotificationNames = GetBehaviorRelayNames()
            };

        public StopRelay GetStopRelayRequest(StateMachineId hostId)
            => new StopRelay()
            {
                BehaviorId = hostId,
                NotificationNames = GetBehaviorRelayNames()
            };

        public BehaviorId GetBehaviorId(StateMachineId hostId)
            => new BehaviorId(BehaviorType, BehaviorName, $"{hostId.Name}:{hostId.Instance}:{Name}:Do:{new Random().Next()}");

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
    }
}
