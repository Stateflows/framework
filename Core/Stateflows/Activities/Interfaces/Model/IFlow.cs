using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IBaseFlow
    {
        int Weight => 1;
    }

    public interface IBaseFlow<TToken> : IBaseFlow
    { }

    public interface IFlowGuard<TToken> : IBaseFlow<TToken>
    {
        Task<bool> GuardAsync(TToken token);
    }

    public interface IFlowTransformation<TToken, TTransformedToken> : IBaseFlow<TToken>
    {
        Task<TTransformedToken> TransformAsync(TToken token);
    }

    public interface IBaseControlFlow : IBaseFlow
    { }

    public interface IControlFlowGuard : IBaseControlFlow
    {
        Task<bool> GuardAsync();
    }
}
