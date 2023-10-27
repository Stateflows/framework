using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ControlFlow
    {
        public IFlowContext Context { get; internal set; }

        public virtual Task<bool> GuardAsync()
            => Task.FromResult(true);
    }

    public abstract class ObjectFlow<TToken> : ControlFlow
        where TToken : Token, new()
    {
        public virtual int Weight => 1;

        new public IFlowContext<TToken> Context { get; internal set; }
    }

    public abstract class ObjectTransformationFlow<TToken, TTransformedToken> : ControlFlow
        where TToken : Token, new()
        where TTransformedToken : Token, new()
    {
        public virtual int Weight => 1;

        new public IFlowContext<TToken> Context { get; internal set; }

        public abstract Task<TTransformedToken> TransformAsync();
    }
}
