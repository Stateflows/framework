using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class TokenPipelineContext : BaseContext, IRootContext
    {
        public TokenPipelineContext(BaseContext context, Edge edge, Token token)
            : base(context)
        {
            Edge = edge;
            Token = token;
        }

        internal readonly Edge Edge;

        public readonly Token Token;

        public bool TokenConsumed = false;

        public ISourceNodeContext sourceNode = null;
        public ISourceNodeContext SourceNode
            => sourceNode ??= new SourceNodeContext(Edge.Source, Context, NodeScope.ThreadId);

        public INodeContext targetNode = null;
        public INodeContext TargetNode
            => targetNode ??= new NodeContext(Edge.Target, Context);
    }
}
