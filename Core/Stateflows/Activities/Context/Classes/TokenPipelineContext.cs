﻿using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class TokenPipelineContext : BaseContext, IRootContext
    {
        public TokenPipelineContext(BaseContext context, Edge edge, TokenHolder token)
            : base(context)
        {
            Edge = edge;
            Token = token;
        }

        internal readonly Edge Edge;

        public readonly TokenHolder Token;

        public bool TokenConsumed = false;

        public ISourceNodeContext sourceNode = null;
        public ISourceNodeContext SourceNode
            => sourceNode ??= new SourceNodeContext(Edge.Source, Context, NodeScope);

        public INodeContext targetNode = null;
        public INodeContext TargetNode
            => targetNode ??= new NodeContext(Edge.Target, null, Context, NodeScope);
    }
}
