using System.Collections.Generic;

namespace Stateflows.StateMachines.Models
{
    internal class Region
    {
        public Graph Graph { get; set; }
        public Vertex ParentVertex { get; set; }
        public string OriginStateMachineName { get; set; } = null;
        public string InitialVertexName { get; set; }
        public Vertex InitialVertex { get; set; }
        public Vertex History { get; set; }
        public Dictionary<string, Vertex> Vertices { get; set; } = new Dictionary<string, Vertex>();

        private bool? isHistoryEnabled = null;
        public bool IsHistoryEnabled
        {
            get
            {
                if (isHistoryEnabled == null)
                {
                    var currentVertex = this;
                    while (currentVertex != null)
                    {
                        if (currentVertex.History != null)
                        {
                            isHistoryEnabled = true;
                        }

                        currentVertex = currentVertex?.ParentVertex?.ParentRegion;
                    }

                    isHistoryEnabled = false;
                }

                return isHistoryEnabled.Value;
            }
        }
        
        public bool HasHistory => History != null;
    }
}
