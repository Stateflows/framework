using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityNodeContext : BaseContext, IActivityNodeInspectionContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        IActivityInspectionContext IActivityNodeInspectionContext.Activity => Activity;

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
            => currentNode ??= new NodeContext(Node, Context);
    }
}
