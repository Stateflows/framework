using System.Collections.Generic;
using System.Linq;

namespace Stateflows.StateMachines.Models
{
    internal class Region
    {
        internal string? SourcePath { get; set; }

        public Graph Graph { get; set; }
        public Vertex ParentVertex { get; set; }
        public string Identifier => $"region:{ParentVertex.Identifier.Length}:{ParentVertex.Identifier}:{ParentVertex.Regions.IndexOf(this)}";
        public string OriginStateMachineName { get; set; } = null;
        public string InitialVertexName { get; set; }
        public Vertex InitialVertex { get; set; }
        public Dictionary<string, Vertex> Vertices { get; set; } = new Dictionary<string, Vertex>();

        public bool IsHistoryEnabled
        {
            get
            {
                for (var region = this; region != null; region = region.ParentVertex?.ParentRegion)
                {
                    if (region.HasHistory) return true;
                }
                return false;
            }
        }
        
        public bool HasHistory => Vertices.Values.Any(vertex => vertex.Type == VertexType.History);
    }
}
