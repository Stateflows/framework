using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Registration.Builders
{
    internal class DecisionBuilder<TToken> : NodeBuilder, IDecisionBuilder<TToken>
    {
        public DecisionBuilder(NodeBuilder builder)
            : base(builder.Node, builder.ActivityBuilder, builder.Services)
        { }

        public IDecisionBuilder<TToken> AddFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
            => AddFlow<TToken>(targetNodeName, buildAction) as IDecisionBuilder<TToken>;

        public IDecisionBuilder<TToken> AddElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null)
            => AddElseFlow<TToken>(targetNodeName, buildAction) as IDecisionBuilder<TToken>;
    }
}
