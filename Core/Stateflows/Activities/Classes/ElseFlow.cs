using System.Threading.Tasks;

namespace Stateflows.Activities
{
    //public abstract class ElseControlFlow : BaseControlFlow
    //{
    //    public override sealed Task<bool> GuardAsync()
    //        => Task.FromResult(true);
    //}

    //public abstract class ElseFlow<TToken> : BaseFlow<TToken>
    //{
    //    public override sealed Task<bool> GuardAsync()
    //        => Task.FromResult(true);
    //}

    public abstract class ElseTransformationFlow<TToken, TTransformedToken> : BaseTransformationFlow<TToken, TTransformedToken>
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);
    }
}
