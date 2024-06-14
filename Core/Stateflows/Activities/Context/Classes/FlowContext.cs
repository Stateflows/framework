using System;
using Stateflows.Activities.Models;
using Stateflows.Activities.Streams;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Engine;

namespace Stateflows.Activities.Context.Classes
{
    internal class FlowContext : BaseContext,
        IActivityFlowContext,
        IIncomingFlowContext,
        IRootContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        protected readonly Edge edge;

        private readonly TokenPipelineContext context;

        private Stream stream;
        protected Stream Stream => stream ??= Context.GetStream(edge.Identifier, NodeScope.ThreadId);

        public FlowContext(TokenPipelineContext context)
            : base(context)
        {
            this.context = context;
            edge = context.Edge;
        }

        public FlowContext(RootContext context, NodeScope nodeScope, Edge edge)
            : base(context, nodeScope)
        {
            this.edge = edge;
        }

        public ISourceNodeContext SourceNode => context.SourceNode;

        public INodeContext TargetNode => context.TargetNode;

        public Type TokenType => edge.TokenType;

        public int Weight => edge.Weight;

        public int TokenCount => Stream.Tokens.Count;

        public bool Activated => Stream.IsActivated;
    }
}
