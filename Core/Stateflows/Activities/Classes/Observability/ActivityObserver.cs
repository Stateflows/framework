using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ActivityObserver : IActivityObserver
    {
        public virtual void BeforeActivityInitialize(IActivityInitializationContext context)
        { }
        public virtual void AfterActivityInitialize(IActivityInitializationContext context, bool initialized)
        { }

        public virtual void BeforeActivityFinalize(IActivityFinalizationContext context)
        { }
        public virtual void AfterActivityFinalize(IActivityFinalizationContext context)
        { }

        public virtual void BeforeNodeInitialize(IActivityNodeContext context)
        { }
        public virtual void AfterNodeInitialize(IActivityNodeContext context)
        { }

        public virtual void BeforeNodeFinalize(IActivityNodeContext context)
        { }
        public virtual void AfterNodeFinalize(IActivityNodeContext context)
        { }

        public virtual void BeforeNodeActivate(IActivityNodeContext context, bool activated)
        { }
        public virtual void AfterNodeActivate(IActivityNodeContext context)
        { }

        public virtual void BeforeNodeExecute(IActivityNodeContext context)
        { }
        public virtual void AfterNodeExecute(IActivityNodeContext context)
        { }

        public virtual void BeforeFlowActivate(IActivityFlowContext context)
        { }
        public virtual void AfterFlowActivate(IActivityFlowContext context, bool activated)
        { }

        public virtual void BeforeFlowGuard<TToken>(IGuardContext<TToken> context)
        { }

        public virtual void AfterFlowGuard<TToken>(IGuardContext<TToken> context, bool guardResult)
        { }

        public virtual void BeforeFlowTransform<TToken, TTransformedToken>(ITransformationContext<TToken> context)
        { }
        
        public virtual void AfterFlowTransform<TToken, TTransformedToken>(ITransformationContext<TToken, TTransformedToken> context)
        { }
    }
}
