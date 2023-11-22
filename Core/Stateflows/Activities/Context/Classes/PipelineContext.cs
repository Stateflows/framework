using System.Threading;
using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class PipelineContext : BaseContext, IPipelineContext, IRootContext
    {
        public PipelineContext(RootContext context, NodeScope nodeScope, Edge edge, IEnumerable<Token> tokens)
            : base(context, nodeScope)
        {
            Edge = edge;
            Tokens = tokens;
        }

        private Edge Edge { get; }

        public CancellationToken CancellationToken => Context.Executor.GetCancellationToken(Edge.Source);

        public IEnumerable<Token> Tokens { get; set; }

        public INodeContext sourceNode = null;
        public INodeContext SourceNode => sourceNode ??= new NodeContext(Edge.Source, Context);

        public INodeContext targetNode = null;
        public INodeContext TargetNode => targetNode ??= new NodeContext(Edge.Target, Context);
        
        IActivityContext IActivityActionContext.Activity => Activity;
    }
}
