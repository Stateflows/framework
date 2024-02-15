using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public abstract class ElseControlFlow : BaseControlFlow
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);
    }

    public abstract class ElseFlow<TToken> : BaseFlow<TToken>
        where TToken : Token, new()
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);
    }

    public abstract class ElseObjectTransformationFlow<TToken, TTransformedToken> : BaseTokenTransformationFlow<TToken, TTransformedToken>
        where TToken : Token, new()
        where TTransformedToken : Token, new()
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);
    }
}
