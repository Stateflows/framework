using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Action context that exposes the available input tokens as a typed sequence.</summary>
    /// <typeparam name="TToken">The token type available to the action.</typeparam>
    public interface ITokenContext<out TToken> : IActivityActionContext
    {
        /// <summary>The input tokens of type <typeparamref name="TToken"/> available to this action.</summary>
        IEnumerable<TToken> Tokens { get; }
    }
}
