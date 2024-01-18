using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Data;

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

    public abstract class BaseTokenFlow<TToken> : BaseControlFlow
        where TToken : Token, new()
    {
        public virtual int Weight => 1;

        new public IFlowContext<TToken> Context { get; internal set; }
    }

    public abstract class TokenFlow<TToken> : BaseTokenFlow<TToken>
        where TToken : Token, new()
    { }

    public abstract class DataFlow<TTokenPayload> : TokenFlow<Token<TTokenPayload>>
    { }

    public abstract class BaseTokenTransformationFlow<TToken, TTransformedToken> : BaseControlFlow
        where TToken : Token, new()
        where TTransformedToken : Token, new()
    {
        public virtual int Weight => 1;

        new public IFlowContext<TToken> Context { get; internal set; }

        public abstract Task<TTransformedToken> TransformAsync();
    }

    public abstract class TokenTransformationFlow<TToken, TTransformedToken> : BaseTokenTransformationFlow<TToken, TTransformedToken>
        where TToken : Token, new()
        where TTransformedToken : Token, new()
    { }

    public abstract class DataTransformationFlow<TTokenPayload, TTransformedTokenPayload> : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
    {
        public abstract Task<TTransformedTokenPayload> TransformPayloadAsync();

        public override async Task<Token<TTransformedTokenPayload>> TransformAsync()
            => (await TransformPayloadAsync()).ToToken();
    }
}
