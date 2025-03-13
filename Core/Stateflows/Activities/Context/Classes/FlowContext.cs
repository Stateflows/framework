using System;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Models;
using Stateflows.Activities.Streams;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class FlowContext : BaseContext,
        IActivityFlowContext,
        IIncomingFlowContext,
        IRootContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;
        
        IBehaviorContext IBehaviorActionContext.Behavior => Activity;

        protected readonly Edge edge;

        private readonly TokenPipelineContext context;

        private Stream stream;
        protected Stream Stream => stream ??= Context.GetStream(edge.Identifier, NodeScope.ThreadId);

        public FlowContext(TokenPipelineContext context)
            : base(context)
        {
            this.context = context;
            edge = context.Edge;
            SourceNode = this.context.SourceNode;
            TargetNode = this.context.TargetNode;
        }

        public FlowContext(RootContext context, NodeScope nodeScope, Edge edge)
            : base(context, nodeScope)
        {
            this.edge = edge;
            SourceNode = new SourceNodeContext(edge.Source, context, nodeScope);
            TargetNode = new NodeContext(edge.Target, context, nodeScope);
        }

        public ISourceNodeContext SourceNode { get; private set; }

        public INodeContext TargetNode { get; private set; }

        public Type TokenType => edge.TokenType;

        public int Weight => edge.Weight;

        public int TokenCount => Stream.Tokens.Count;

        public bool Activated => Stream.IsActivated;
    }
}
