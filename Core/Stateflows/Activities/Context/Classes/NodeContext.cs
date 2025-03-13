using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Engine;

namespace Stateflows.Activities.Context.Classes
{
    internal class NodeContext : INodeContext
    {
        public readonly Node Node;

        public string Name => Node.Name;

        public NodeType NodeType => Node.Type;

        internal RootContext Context { get; }

        internal NodeScope NodeScope { get; }

        private IEnumerable<IIncomingFlowContext> incomingFlows;
        public IEnumerable<IIncomingFlowContext> IncomingFlows
            => incomingFlows ??= Node.IncomingEdges.Select(edge => new FlowContext(Context, NodeScope, edge));

        private IEnumerable<IFlowContext> outgoingFlows;
        public IEnumerable<IFlowContext> OutgoingFlows
            => outgoingFlows ??= Node.Edges.Select(edge => new FlowContext(Context, NodeScope, edge));

        public NodeContext(Node node, RootContext context, NodeScope nodeScope)
        {
            Node = node;
            Context = context;
            NodeScope = nodeScope;
        }

        public bool TryGetParentNode(out INodeContext parentNodeContext)
        {
            var parent = Node.Parent;

            parentNodeContext = parent != null
                ? new NodeContext(parent, Context, null)
                : null;

            return parentNodeContext != null;
        }
    }
}
