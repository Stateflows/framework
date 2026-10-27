using System.Collections.Generic;

namespace Stateflows.Activities
{
    /// <summary>Provides a mutable collection of output tokens of a particular type.</summary>
    /// <typeparam name="TToken">The token payload type.</typeparam>
    /// <remarks>Changes stage tokens for the current activity execution; the activity runtime forwards them through the connected flows.</remarks>
    public interface IOutputTokens<TToken> : ICollection<TToken>
    {
        /// <summary>Adds all supplied tokens to the activity output.</summary>
        /// <param name="items">The tokens to stage for output.</param>
        void AddRange(IEnumerable<TToken> items);
    }
}
