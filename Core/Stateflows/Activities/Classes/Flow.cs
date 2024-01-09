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

    public abstract class BaseObjectFlow<TToken> : BaseControlFlow
        where TToken : Token, new()
    {
        public virtual int Weight => 1;

        new public IFlowContext<TToken> Context { get; internal set; }
    }

    public abstract class ObjectFlow<TToken> : BaseObjectFlow<TToken>
        where TToken : Token, new()
    { }

    public abstract class BaseObjectTransformationFlow<TToken, TTransformedToken> : BaseControlFlow
        where TToken : Token, new()
        where TTransformedToken : Token, new()
    {
        public virtual int Weight => 1;

        new public IFlowContext<TToken> Context { get; internal set; }

        public abstract Task<TTransformedToken> TransformAsync();
    }

    public abstract class ObjectTransformationFlow<TToken, TTransformedToken> : BaseObjectTransformationFlow<TToken, TTransformedToken>
        where TToken : Token, new()
        where TTransformedToken : Token, new()
    { }
}
