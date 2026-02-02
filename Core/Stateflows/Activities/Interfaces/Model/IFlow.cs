using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IEdge : IActivityElement
    {
        static virtual int Weight => 1;
    }

    public interface IFlow<in TToken> : IEdge;

    public interface IFlowGuard<in TToken> : IFlow<TToken>
    {
        Task<bool> GuardAsync(TToken token);
    }

    public interface IFlowGuard : IControlFlowGuard, IFlowGuard<object>
    {
        Task<bool> IFlowGuard<object>.GuardAsync(object token)
            => GuardAsync();
    }

    public interface IActivityGuard : IFlowGuard;

    public interface IFlowTransformation<in TToken, TTransformedToken> : IFlow<TToken>
    {
        Task<TTransformedToken> TransformAsync(TToken token);
    }
}
