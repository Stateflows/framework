using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;
using System.Collections.Generic;

namespace Stateflows.Testing.Activities.Cradle.Context
{
    internal class NodeContext : INodeContext
    {
        public string NodeName { get; internal set; }

        public NodeType NodeType { get; internal set; }

        public INodeContext ParentNode { get; internal set; }

        public IEnumerable<IIncomingFlowContext> IncomingFlows { get; internal set; }

        public IEnumerable<IFlowContext> OutgoingFlows { get; internal set; }

        public bool TryGetParentNode(out INodeContext parentNodeContext)
            => (parentNodeContext = ParentNode) != null;
    }
}
