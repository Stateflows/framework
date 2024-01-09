using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class TokenFlowContext<TToken> : BaseContext,
        IGuardContext,
        ITransformationContext<TToken>,
        IRootContext
        where TToken : Token, new()
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        private readonly Edge Edge;

        public TokenFlowContext(TokenPipelineContext context, TToken token)
            : base(context)
        {
            Edge = context.Edge;
            Token = token;
        }

        public TToken Token { get; private set; }

        public ISourceNodeContext sourceNode = null;
        public ISourceNodeContext SourceNode
            => sourceNode ??= new SourceNodeContext(Edge.Source, Context, NodeScope.ThreadId);

        private INodeContext targetNode = null;
        public INodeContext TargetNode
            => targetNode ??= new NodeContext(Edge.Target, Context);
    }
}
