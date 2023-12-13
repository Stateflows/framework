using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class NodeContext : INodeContext
    {
        public Node Node { get; set; }

        public string NodeName => Node.Name;

        private RootContext Context { get; }

        public NodeContext(Node node, RootContext context)
        {
            Node = node;
            Context = context;
        }

        public bool TryGetParentNode(out INodeContext parentNodeContext)
        {
            var parent = Node.Parent;

            parentNodeContext = parent != null
                ? new NodeContext(parent, Context)
                : null;

            return parentNodeContext != null;
        }
    }
}
