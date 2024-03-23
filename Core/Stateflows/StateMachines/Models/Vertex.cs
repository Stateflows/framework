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
        FinalState,
        Pseudostate
    }

    internal class Vertex
    {
        public Graph Graph { get; set; }
        public Vertex Parent { get; set; }
        public string Name { get; set; }
        public VertexType Type { get; set; }
        public string Identifier => Name;

        private Logic<StateMachineActionAsync> initialize = null;
        public Logic<StateMachineActionAsync> Initialize => initialize ??= new Logic<StateMachineActionAsync>() { Name = Constants.Initialize };

        private Logic<StateMachineActionAsync> finalize = null;
        public Logic<StateMachineActionAsync> Finalize => finalize ??= new Logic<StateMachineActionAsync>() { Name = Constants.Finalize };

        private Logic<StateMachineActionAsync> entry = null;
        public Logic<StateMachineActionAsync> Entry => entry ??= new Logic<StateMachineActionAsync>() { Name = Constants.Entry };

        private Logic<StateMachineActionAsync> exit = null;
        public Logic<StateMachineActionAsync> Exit => exit ??= new Logic<StateMachineActionAsync>() { Name = Constants.Exit };

        public Dictionary<string, Edge> Edges { get; set; } = new Dictionary<string, Edge>();
        public IEnumerable<Edge> OrderedEdges => Edges.Values.OrderBy(edge => edge.IsElse);
        public string InitialVertexName { get; set; }
        public Vertex InitialVertex { get; set; }
        public Dictionary<string, Vertex> Vertices { get; set; } = new Dictionary<string, Vertex>();
        public List<string> DeferredEvents { get; set; } = new List<string>();

        public StateActionInitializationBuilder BehaviorInitializationBuilder { get; set; }
        public string BehaviorName { get; set; }
        public string BehaviorType { get; set; }
        public List<Type> BehaviorSubscriptions { get; set; } = new List<Type>();
        public List<string> GetBehaviorSubscriptionNames()
            => BehaviorSubscriptions
            .Select(t => EventInfo.GetName(t))
            .ToList();

        public SubscriptionRequest GetSubscriptionRequest(StateMachineId hostId)
            => new SubscriptionRequest()
            {
                BehaviorId = hostId,
                NotificationNames = GetBehaviorSubscriptionNames()
            };

        public UnsubscriptionRequest GetUnsubscriptionRequest(StateMachineId hostId)
            => new UnsubscriptionRequest()
            {
                BehaviorId = hostId,
                NotificationNames = GetBehaviorSubscriptionNames()
            };

        public BehaviorId GetBehaviorId(StateMachineId hostId)
            => new BehaviorId(BehaviorType, BehaviorName, $"__stateBehavior:{hostId.Name}:{hostId.Instance}:{Name}");
    }
}
