using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public abstract class ElseControlFlow : BaseControlFlow
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);
    }

    public abstract class ElseObjectFlow<TToken> : BaseObjectFlow<TToken>
        where TToken : Token, new()
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);
    }

    public abstract class ElseObjectTransformationFlow<TToken, TTransformedToken> : BaseObjectTransformationFlow<TToken, TTransformedToken>
        where TToken : Token, new()
        where TTransformedToken : Token, new()
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);
    }
}
