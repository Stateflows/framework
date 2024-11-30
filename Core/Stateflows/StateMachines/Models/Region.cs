using System.Linq;
using System.Collections.Generic;

namespace Stateflows.StateMachines.Models
{
    internal class Region
    {
        public Graph Graph { get; set; }
        public Vertex ParentVertex { get; set; }
        public string OriginStateMachineName { get; set; } = null;

        public Dictionary<string, Edge> Edges { get; set; } = new Dictionary<string, Edge>();
        public IEnumerable<Edge> OrderedEdges => Edges.Values.OrderBy(edge => edge.IsElse);
        public string InitialVertexName { get; set; }
        public Vertex InitialVertex { get; set; }
        public Dictionary<string, Vertex> Vertices { get; set; } = new Dictionary<string, Vertex>();
    }
}
