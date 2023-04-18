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

        private Action<StateMachineActionAsync> initialize = null;
        public Action<StateMachineActionAsync> Initialize => initialize ?? (initialize = new Action<StateMachineActionAsync>() { Name = Constants.Initialize, Graph = Graph });

        private Action<StateMachineActionAsync> entry = null;
        public Action<StateMachineActionAsync> Entry => entry ?? (entry = new Action<StateMachineActionAsync>() { Name = Constants.Entry, Graph = Graph });

        private Action<StateMachineActionAsync> exit = null;
        public Action<StateMachineActionAsync> Exit => exit ?? (exit = new Action<StateMachineActionAsync>() { Name = Constants.Exit, Graph = Graph });

        public List<Edge> Edges { get; set; } = new List<Edge>();
        public string InitialVertexName { get; set; }
        public Vertex InitialVertex { get; set; }
        public Dictionary<string, Vertex> Vertices { get; set; } = new Dictionary<string, Vertex>();
    }
}
