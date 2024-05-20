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
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);
    }

    public abstract class ElseObjectTransformationFlow<TToken, TTransformedToken> : BaseTransformationFlow<TToken, TTransformedToken>
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);
    }
}
