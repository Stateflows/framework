using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface INodeContext
    {
        /// <summary>
        /// Name of the node
        /// </summary>
        string NodeName { get; }

        /// <summary>
        /// Type of the node
        /// </summary>
        NodeType NodeType { get; }

        /// <summary>
        /// Collection of flows incoming to the node
        /// </summary>
        IEnumerable<IIncomingFlowContext> IncomingFlows { get; }

        /// <summary>
        /// Collection of flows outgoing from the node
        /// </summary>
        IEnumerable<IFlowContext> OutgoingFlows { get; }

        /// <summary>
        /// Provides access to parent node (if available)
        /// </summary>
        /// <param name="parentNodeContext">out parameter with reference to parent node information</param>
        /// <returns>True when parent node is available, false otherwise</returns>
        bool TryGetParentNode(out INodeContext parentNodeContext);
    }
}
