using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    public interface IEdge : IActivityElement
    {
        static virtual int Weight => 1;
    }

    public interface IFlow<in TToken> : IEdge;

    public interface IFlowGuard<in TToken> : IFlow<TToken>, IAbstractGuard<TToken>;

    public interface IFlowGuard : IControlFlowGuard, IFlowGuard<object>
    {
        Task<bool> IAbstractGuard<object>.GuardAsync(object token)
            => GuardAsync();
    }

    public interface IActivityGuard : IActivityGuard<object>, IFlowGuard;
    
    public interface IActivityGuard<in TToken> : IFlowGuard<TToken>;

    public interface IFlowTransformation<in TToken, TTransformedToken> : IFlow<TToken>
    {
        Task<TTransformedToken> TransformAsync(TToken token);
    }
}
