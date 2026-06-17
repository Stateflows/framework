using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Builders
{
    internal class DecisionBuilder<TToken> : NodeBuilder,
        IDecisionBuilder<TToken>,
        IOverridenDecisionBuilder<TToken>
    {
        public DecisionBuilder(NodeBuilder builder)
            : base(builder.Node, builder.ActivityBuilder)
        { }

        public IDecisionBuilder<TToken> AddFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
            => AddFlow<TToken>(targetNodeName, buildAction) as IDecisionBuilder<TToken>;

        public IDecisionBuilder<TToken> AddElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null)
            => AddElseFlow<TToken>(targetNodeName, buildAction) as IDecisionBuilder<TToken>;

        IOverridenDecisionBuilder<TToken> IDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>.AddFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow(targetNodeName, buildAction) as IOverridenDecisionBuilder<TToken>;

        IOverridenDecisionBuilder<TToken> IElseDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>.AddElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction)
            => AddElseFlow(targetNodeName, buildAction) as IOverridenDecisionBuilder<TToken>;

        IOverridenDecisionBuilder<TToken> IOverridenDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>.UseFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => UseFlow(targetNodeName, buildAction) as IOverridenDecisionBuilder<TToken>;

        IOverridenDecisionBuilder<TToken> IOverridenElseDecisionFlowBase<TToken, IOverridenDecisionBuilder<TToken>>.UseElseFlow(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction)
            => UseElseFlow(targetNodeName, buildAction) as IOverridenDecisionBuilder<TToken>;
    }
}
