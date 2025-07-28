using System.Collections.Generic;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface INodeInspection
    {
        string Name { get; }

        NodeType Type { get; }

        bool Active { get; }
        
        NodeStatus Status { get; }

        IEnumerable<IFlowInspection> Flows { get; }

        IEnumerable<INodeInspection> Nodes { get; }
    }
}
