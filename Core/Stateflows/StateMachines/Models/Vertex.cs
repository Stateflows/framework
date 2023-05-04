using System.Collections.Generic;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Models
{
    internal class Vertex
    {
        public Graph Graph { get; set; }
        public Vertex Parent { get; set; }
        public string Name { get; set; }

        private Logic<StateMachineActionAsync> initialize = null;
        public Logic<StateMachineActionAsync> Initialize => initialize ?? (initialize = new Logic<StateMachineActionAsync>() { Name = Constants.Initialize, Graph = Graph });

        private Logic<StateMachineActionAsync> entry = null;
        public Logic<StateMachineActionAsync> Entry => entry ?? (entry = new Logic<StateMachineActionAsync>() { Name = Constants.Entry, Graph = Graph });

        private Logic<StateMachineActionAsync> exit = null;
        public Logic<StateMachineActionAsync> Exit => exit ?? (exit = new Logic<StateMachineActionAsync>() { Name = Constants.Exit, Graph = Graph });

        public List<Edge> Edges { get; set; } = new List<Edge>();
        public string InitialVertexName { get; set; }
        public Vertex InitialVertex { get; set; }
        public Dictionary<string, Vertex> Vertices { get; set; } = new Dictionary<string, Vertex>();
        public List<string> DeferredEvents { get; set; } = new List<string>();

        public Dictionary<string, object> SubmachineInitialValues { get; set; }
        public string SubmachineName { get; set; }
    }
}
