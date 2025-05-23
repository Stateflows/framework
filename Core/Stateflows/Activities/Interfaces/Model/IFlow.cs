using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IEdge
    {
        int Weight => 1;
    }

    public interface IFlow<TToken> : IEdge
    { }

    public interface IFlowGuard<TToken> : IFlow<TToken>
    {
        Task<bool> GuardAsync(TToken token);
    }

    public interface IFlowGuard : IControlFlowGuard, IFlowGuard<object>
    {
        Task<bool> IFlowGuard<object>.GuardAsync(object token)
            => GuardAsync();
    }

    public interface IFlowTransformation<TToken, TTransformedToken> : IFlow<TToken>
    {
        Task<TTransformedToken> TransformAsync(TToken token);
    }
}
