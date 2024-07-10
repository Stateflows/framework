using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class BaseControlFlow
    {
        public IActivityFlowContext Context => ActivityFlowContextAccessor.Context.Value;

        public virtual Task<bool> GuardAsync()
            => Task.FromResult(true);
    }

    public abstract class ControlFlow : BaseControlFlow
    { }

    public abstract class BaseFlow<TToken> : BaseControlFlow
    {
        public virtual int Weight => 1;

        new public IActivityFlowContext<TToken> Context
            => (IActivityFlowContext<TToken>)base.Context;
    }

    public abstract class Flow<TToken> : BaseFlow<TToken>//, IGuardFlow<TToken>
    { }

    public abstract class BaseTransformationFlow<TToken, TTransformedToken> : BaseControlFlow
    {
        public virtual int Weight => 1;

        new public IActivityFlowContext<TToken> Context
            => (IActivityFlowContext<TToken>)base.Context;

        public abstract Task<TTransformedToken> TransformAsync();
    }

    public abstract class TransformationFlow<TToken, TTransformedToken> : BaseTransformationFlow<TToken, TTransformedToken>//, ITransformationFlow<TToken, TTransformedToken>
    {
        public abstract Task<TTransformedToken> TransformAsync(TToken token);
    }
}
