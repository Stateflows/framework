using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityNodeContext : BaseContext, IActivityNodeContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;
        
        IBehaviorContext IBehaviorActionContext.Behavior => Activity;

        internal Node Node { get; }
        internal Edge Edge { get; }

        public ActivityNodeContext(BaseContext context, Node node, Edge edge)
            : base(context.Context, context.NodeScope)
        {
            Node = node;
            Edge = edge;
        }

        public ActivityNodeContext(RootContext context, NodeScope nodeScope, Node node)
            : base(context, nodeScope)
        {
            Node = node;
            if (node.Type != NodeType.Input && node.Type != NodeType.Initial)
            {
                Edge = nodeScope.Edge;
            }
        }

        private ICurrentNodeContext currentNode = null;
        ICurrentNodeContext IActivityNodeContext.Node
            => currentNode ??= new NodeContext(Node, Edge, Context, NodeScope);
    }
}
