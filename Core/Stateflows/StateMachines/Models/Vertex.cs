using System.Collections.Generic;
using System.Linq;
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
        public string Identifier => (Parent is null)
            ? Name
            : $"{Parent.Identifier}:{Name}";

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

        public Dictionary<string, object> SubmachineInitialValues { get; set; }
        public StateActionInitializationBuilder SubmachineInitializationBuilder { get; set; }
        public string SubmachineName { get; set; }
    }
}
