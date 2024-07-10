using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IBaseFlow
    {
        int Weight => 1;
    }

    public interface IFlow : IBaseFlow
    { }

    public interface IFlowGuard<TToken> : IFlow
    {
        Task<bool> GuardAsync(TToken token);
    }

    public interface IFlowTransformation<TToken, TTransformedToken> : IFlow
    {
        Task<TTransformedToken> TransformAsync(TToken token);
    }

    public interface IControlFlow : IBaseFlow
    { }

    public interface IControlFlowGuard : IControlFlow
    {
        Task<bool> GuardAsync();
    }
}
