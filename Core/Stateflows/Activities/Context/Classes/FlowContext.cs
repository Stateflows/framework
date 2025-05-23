using System;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Models;
using Stateflows.Activities.Streams;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Context.Classes
{
    internal class FlowContext : BaseContext,
        IActivityFlowContext,
        IIncomingFlowContext,
        IActivityBeforeFlowContext,
        IActivityAfterFlowContext,
        IRootContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;
        
        IBehaviorContext IBehaviorActionContext.Behavior => Activity;

        internal readonly Edge Edge;

        private readonly TokenPipelineContext context;

        private Stream stream;
        protected Stream Stream => stream ??= Context.GetStream(Edge.Identifier, NodeScope.ThreadId);

        public FlowContext(TokenPipelineContext context)
            : base(context)
        {
            this.context = context;
            Edge = context.Edge;
            SourceNode = this.context.SourceNode;
            TargetNode = this.context.TargetNode;
        }

        public FlowContext(RootContext context, NodeScope nodeScope, Edge edge)
            : base(context, nodeScope)
        {
            Edge = edge;
            SourceNode = new SourceNodeContext(edge.Source, context, nodeScope);
            TargetNode = new NodeContext(edge.Target, null, context, nodeScope);
        }

        public ISourceNodeContext SourceNode { get; private set; }

        public INodeContext TargetNode { get; private set; }

        public Type TokenType => Edge.TokenType;
        
        public Type TargetTokenType => Edge.TargetTokenType;

        public int Weight => Edge.Weight;

        public int TokenCount { get; set; }

        public int TargetTokenCount { get; set; }

        public bool Activated => Stream.IsActivated;
    }
}
