using System;
using System.Collections.Generic;

namespace Stateflows.Activities.Inspection.Interfaces
{
    /// <summary>Read-only inspection data for an activity node.</summary>
    public interface INodeInspection
    {
        /// <summary>The node's graph name.</summary>
        string Name { get; }

        /// <summary>The node's kind.</summary>
        NodeType Type { get; }

        /// <summary>The declared token type of a typed boundary pin, or null for other nodes.</summary>
        Type PinTokenType => null;

        /// <summary>Whether the node is active.</summary>
        bool Active { get; }
        
        /// <summary>The node's runtime status.</summary>
        NodeStatus Status { get; }

        /// <summary>The node's outgoing flows.</summary>
        IEnumerable<IFlowInspection> Flows { get; }

        /// <summary>The child nodes contained within this node, if any.</summary>
        IEnumerable<INodeInspection> Nodes { get; }
    }
}
