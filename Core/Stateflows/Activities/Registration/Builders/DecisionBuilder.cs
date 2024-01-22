﻿using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Registration.Builders
{
    internal class DecisionBuilder<TToken> : NodeBuilder, IDecisionBuilder<TToken>
        where TToken : Token, new()
    {
        public DecisionBuilder(NodeBuilder builder)
            : base(builder.Node, builder.ActivityBuilder, builder.Services)
        { }

        public IDecisionBuilder<TToken> AddFlow(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction = null)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as IDecisionBuilder<TToken>;

        public IDecisionBuilder<TToken> AddElseFlow(string targetNodeName, ElseObjectFlowBuilderAction<TToken> buildAction = null)
            => AddElseTokenFlow<TToken>(targetNodeName, buildAction) as IDecisionBuilder<TToken>;
    }
}