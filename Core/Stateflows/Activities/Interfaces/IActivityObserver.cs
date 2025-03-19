using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityObserver
    {
        void BeforeActivityInitialize(IActivityInitializationContext context);
        void AfterActivityInitialize(IActivityInitializationContext context, bool initialized);

        void BeforeActivityFinalize(IActivityFinalizationContext context);
        void AfterActivityFinalize(IActivityFinalizationContext context);

        void BeforeNodeInitialize(IActivityNodeContext context);
        void AfterNodeInitialize(IActivityNodeContext context);

        void BeforeNodeFinalize(IActivityNodeContext context);
        void AfterNodeFinalize(IActivityNodeContext context);

        void BeforeNodeActivate(IActivityNodeContext context, bool activated);
        void AfterNodeActivate(IActivityNodeContext context);

        void BeforeNodeExecute(IActivityNodeContext context);
        void AfterNodeExecute(IActivityNodeContext context);

        void BeforeFlowActivate(IActivityBeforeFlowContext context);
        void AfterFlowActivate(IActivityAfterFlowContext context, bool activated);

        void BeforeFlowGuard<TToken>(IGuardContext<TToken> context);
        void AfterFlowGuard<TToken>(IGuardContext<TToken> context, bool guardResult);

        void BeforeFlowTransform<TToken, TTransformedToken>(ITransformationContext<TToken> context);
        void AfterFlowTransform<TToken, TTransformedToken>(ITransformationContext<TToken, TTransformedToken> context);
    }
}
