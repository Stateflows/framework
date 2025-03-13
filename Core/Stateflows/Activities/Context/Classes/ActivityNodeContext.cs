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

        public ActivityNodeContext(BaseContext context, Node node)
            : base(context.Context, context.NodeScope)
        {
            Node = node;
        }

        public ActivityNodeContext(RootContext context, NodeScope nodeScope, Node node)
            : base(context, nodeScope)
        {
            Node = node;
        }

        private INodeContext currentNode = null;
        public INodeContext CurrentNode
            => currentNode ??= new NodeContext(Node, Context, NodeScope);
    }
}
