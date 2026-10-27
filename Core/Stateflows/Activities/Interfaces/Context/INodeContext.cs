using System.Collections.Generic;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Describes an activity node and the flows connected to it.</summary>
    public interface INodeContext
    {
        /// <summary>
        /// Name of the node
        /// </summary>
        string Name { get; }

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

    /// <summary>Node context for the node currently executing or evaluating a flow.</summary>
    public interface ICurrentNodeContext : INodeContext
    {
        /// <summary>Attempts to retrieve the incoming flow currently being processed.</summary>
        /// <param name="flowContext">Receives the current incoming flow when one is active.</param>
        /// <returns><see langword="true"/> if a current flow is available; otherwise, <see langword="false"/>.</returns>
        bool TryGetCurrentFlow(out IIncomingFlowContext flowContext);
    }
}
