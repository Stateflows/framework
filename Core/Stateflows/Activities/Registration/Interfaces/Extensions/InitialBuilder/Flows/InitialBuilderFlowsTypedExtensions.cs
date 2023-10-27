using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class InitialBuilderFlowsTypedExtensions
    {
        //public static IActivityBuilder AddInitialControlFlow(this IActivityInitialBuilder builder, string targetNodeName, FlowBuilderAction buildAction = null)
        //    => builder
        //        .AddInitial(b => b.AddControlFlow(targetNodeName, buildAction));

        //public static IActivityBuilder AddInitialFlow<TToken>(this IActivityInitialBuilder builder, string targetNodeName, FlowBuilderAction<TToken> buildAction = null)
        //    where TToken : Token, new()
        //    => builder
        //        .AddInitial(b => b.AddFlow<TToken>(targetNodeName, buildAction));

        //public static IInitialBuilder AddObjectFlow<TToken, TTargetNode>(this IInitialBuilder builder, FlowBuilderAction<TToken> buildAction = null)
        //    where TToken : Token, new()
        //    where TTargetNode : ActivityNode
        //    => builder.AddObjectFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        //public static IInitialBuilder AddObjectFlow<TToken, TObjectFlow>(this IInitialBuilder builder, string targetNodeName)
        //    where TToken : Token, new()
        //    where TObjectFlow : ObjectFlow<TToken>
        //{

        //    var self = builder as IInitialBuilderInternal;
        //    self.Services.RegisterFlow<TObjectFlow, TToken>();
        //    self.AddObjectFlow<TToken>(
        //        targetNodeName,
        //        b => b.AddObjectFlowEvents<TObjectFlow, TToken>()
        //    );

        //    return builder;
        //}

        //public static IInitialBuilder AddObjectFlow<TToken, TObjectFlow, TTargetNode>(this IInitialBuilder builder)
        //    where TToken : Token, new()
        //    where TObjectFlow : ObjectFlow<TToken>
        //    where TTargetNode : ActivityNode
        //    => AddObjectFlow<TToken, TObjectFlow>(builder, ActivityNodeInfo<TTargetNode>.Name);

        //public static IInitialBuilder AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IInitialBuilder builder, string targetNodeName)
        //    where TToken : Token, new()
        //    where TTransformedToken : Token
        //    where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
        //{

        //    var self = builder as IInitialBuilderInternal;
        //    self.Services.RegisterFlow<TObjectTransformationFlow, TToken>();
        //    self.AddObjectFlow<TToken>(
        //        targetNodeName,
        //        b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
        //    );

        //    return builder;
        //}

        //public static IInitialBuilder AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IInitialBuilder builder)
        //    where TToken : Token, new()
        //    where TTransformedToken : Token
        //    where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
        //    where TTargetNode : ActivityNode
        //    => AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow>(builder, ActivityNodeInfo<TTargetNode>.Name);
    }
}
