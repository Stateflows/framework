using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityNodeContext : BaseContext, IActivityNodeContext
    {
        IBehaviorContext IBehaviorActionContext.Behavior => Activity;
        public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
        {
            parentBehaviorContext = Behavior.Context.ContextParentId.HasValue
                ? Behavior
                : null;
            
            return parentBehaviorContext != null;
        }
        public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
        {
            ownerBehaviorContext = Behavior.Context.ContextOwnerId.HasValue
                ? Behavior
                : null;
            
            return ownerBehaviorContext != null;
        }

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

        public object LockHandle => Activity.LockHandle;
        public IReadOnlyTree<INodeContext> ActiveNodes => Activity.ActiveNodes;
    }
}
