using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Base contract for a directed edge in an activity graph.</summary>
    public interface IEdge : IActivityElement
    {
        /// <summary>Gets the default flow weight.</summary>
        static virtual int Weight => 1;
    }

    /// <summary>Marks an object flow carrying values of the specified token type.</summary>
    /// <typeparam name="TToken">The type of token carried by the flow.</typeparam>
    public interface IFlow<in TToken> : IEdge;

    /// <summary>Marks a typed object flow whose eligibility is checked by a guard.</summary>
    /// <typeparam name="TToken">The token type evaluated by the guard.</typeparam>
    public interface IFlowGuard<in TToken> : IFlow<TToken>, IAbstractGuard<TToken>;

    /// <summary>Marks a guarded control flow that does not carry a typed object token.</summary>
    public interface IFlowGuard : IControlFlowGuard, IFlowGuard<object>
    {
        /// <inheritdoc/>
        Task<bool> IAbstractGuard<object>.GuardAsync(object token)
            => GuardAsync();
    }

    /// <summary>Marks a guard applicable to activity-level control flow.</summary>
    public interface IActivityGuard : IActivityGuard<object>, IFlowGuard;
    
    /// <summary>Marks a typed guard applicable to activity-level object flow.</summary>
    /// <typeparam name="TToken">The token type evaluated by the guard.</typeparam>
    public interface IActivityGuard<in TToken> : IFlowGuard<TToken>;

    /// <summary>Defines a flow that transforms an input token into another token type.</summary>
    /// <typeparam name="TToken">The input token type.</typeparam>
    /// <typeparam name="TTransformedToken">The output token type.</typeparam>
    public interface IFlowTransformation<in TToken, TTransformedToken> : IFlow<TToken>
    {
        /// <summary>Transforms a token as it passes through this flow.</summary>
        /// <param name="token">The input token.</param>
        /// <returns>The transformed token.</returns>
        Task<TTransformedToken> TransformAsync(TToken token);
    }
}
