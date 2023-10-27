using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class NodeContext : INodeContext
    {
        public Node Node { get; set; }

        public string NodeName => Node.Name;

        private RootContext Context { get; }

        private ActionValues ActionValues { get; }

        public NodeContext(Node node, RootContext context)
        {
            Node = node;
            Context = context;
            ActionValues = Context.GetActionValues(NodeName);
        }

        public IDictionary<string, object> Values => ActionValues.Values;

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
