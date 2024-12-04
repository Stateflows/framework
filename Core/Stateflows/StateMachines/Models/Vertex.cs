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
    internal enum VertexType
    {
        InitialState,
        State,
        InitialCompositeState,
        CompositeState,
        InitialOrthogonalState,
        OrthogonalState,
        FinalState,
        Junction,
        Choice,
        Join,
        Fork,
    }

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

        public StateActionInitializationBuilderAsync BehaviorInitializationBuilder { get; set; }
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

        public BehaviorId GetBehaviorId(StateMachineId hostId)
            => new BehaviorId(BehaviorType, BehaviorName, $"__stateBehavior:{hostId.Name}:{hostId.Instance}:{Name}");
    }
}
