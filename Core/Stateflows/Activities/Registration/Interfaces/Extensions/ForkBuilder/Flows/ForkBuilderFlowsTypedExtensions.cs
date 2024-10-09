using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ForkBuilderFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IForkBuilder AddFlow<TToken, TTargetNode>(this IForkBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken>(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IForkBuilder AddFlow<TToken, TFlow>(this IForkBuilder builder, string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
        {
            (builder as IInternal).Services.AddServiceType<TFlow>();
            return builder.AddFlow<TToken>(
                targetNodeName,
                b =>
                {
                    b.AddObjectFlowEvents<TFlow, TToken>();
                    buildAction?.Invoke(b);
                }
            );
        }

        [DebuggerHidden]
        public static IForkBuilder AddFlow<TToken, TFlow, TTargetNode>(this IForkBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IForkBuilder AddFlow<TToken, TTransformedToken, TTransformationFlow>(this IForkBuilder builder, string targetNodeName, ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.AddServiceType<TTransformationFlow>();
            return builder.AddFlow<TToken>(
                targetNodeName,
                b =>
                {
                    b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>();
                    buildAction?.Invoke(b as IObjectFlowBuilder<TTransformedToken>);
                }
            );
        }

        [DebuggerHidden]
        public static IForkBuilder AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(this IForkBuilder builder, ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
