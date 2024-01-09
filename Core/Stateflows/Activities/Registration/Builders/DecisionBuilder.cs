using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Registration.Builders
{
    internal class DecisionBuilder<TToken> : NodeBuilder, IDecisionBuilder<TToken>
        where TToken : Token, new()
    {
        public DecisionBuilder(NodeBuilder builder)
            : base(builder.Node, builder.ActivityBuilder, builder.Services)
        { }

        public IDecisionBuilder<TToken> AddObjectFlow(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction = null)
            => AddObjectFlow<TToken>(targetNodeName, buildAction) as IDecisionBuilder<TToken>;

        public IDecisionBuilder<TToken> AddElseObjectFlow(string targetNodeName, ElseObjectFlowBuilderAction<TToken> buildAction = null)
            => AddElseObjectFlow<TToken>(targetNodeName, buildAction) as IDecisionBuilder<TToken>;
    }
}
