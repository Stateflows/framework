using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class FlowContext<TToken> : BaseContext,
        IGuardContext<TToken>,
        IGuardContext,
        ITransformContext<TToken>,
        IRootContext
        where TToken : Token, new()
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        public Edge Edge { get; set; }

        public FlowContext(RootContext context, NodeScope nodeScope, Edge edge, TToken token)
            : base(context, nodeScope)
        {
            Edge = edge;
            Token = token;
        }

        public TToken Token { get; private set; }

        public INodeContext sourceNode = null;
        public INodeContext SourceNode => sourceNode ??= new NodeContext(Edge.Source, Context);

        private INodeContext targetNode = null;
        public INodeContext TargetNode => targetNode ??= new NodeContext(Edge.Target, Context);
    }
}
