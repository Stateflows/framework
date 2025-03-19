using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityNodeContext : BaseContext, IActivityNodeContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;
        
        IBehaviorContext IBehaviorActionContext.Behavior => Activity;

        internal readonly Node Node;
        internal readonly Edge Edge;

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
        public ICurrentNodeContext CurrentNode
            => currentNode ??= new NodeContext(Node, Edge, Context, NodeScope);
    }
}
