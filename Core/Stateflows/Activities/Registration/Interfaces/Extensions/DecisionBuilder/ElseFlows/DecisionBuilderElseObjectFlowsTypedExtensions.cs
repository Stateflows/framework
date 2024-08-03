using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class DecisionBuilderElseObjectFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddElseFlow<TToken, TTargetNode>(this IDecisionBuilder<TToken> builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddElseFlow(ActivityNode<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<TToken>));

        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddElseFlow<TToken, TTransformedToken, TTransformationFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.AddServiceType<TTransformationFlow>();

            return builder.AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()
            );
        }

        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddElseFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => builder.AddElseFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name);
    }
}
