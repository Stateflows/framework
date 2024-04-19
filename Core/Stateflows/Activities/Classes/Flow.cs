using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class BaseControlFlow
    {
        public IFlowContext Context { get; internal set; }

        public virtual Task<bool> GuardAsync()
            => Task.FromResult(true);
    }

    public abstract class ControlFlow : BaseControlFlow
    { }

    public abstract class BaseFlow<TToken> : BaseControlFlow
    {
        public virtual int Weight => 1;

        new public IFlowContext<TToken> Context
            => (IFlowContext<TToken>)base.Context;
    }

    public abstract class Flow<TToken> : BaseFlow<TToken>
    { }

    public abstract class BaseTransformationFlow<TToken, TTransformedToken> : BaseControlFlow
    {
        public virtual int Weight => 1;

        new public IFlowContext<TToken> Context
            => (IFlowContext<TToken>)base.Context;

        public abstract Task<TTransformedToken> TransformAsync();
    }

    public abstract class TransformationFlow<TToken, TTransformedToken> : BaseTransformationFlow<TToken, TTransformedToken>
    { }
}
