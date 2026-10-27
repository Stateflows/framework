using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Interfaces
{
    /// <summary>Exposes mutable token collections for an activity action's current execution scope.</summary>
    /// <remarks>This low-level accessor is intended for integration with the activity token context; prefer the typed input/output APIs in normal action code.</remarks>
    public interface ITokensAccessor
    {
        /// <summary>Tokens supplied to the current action.</summary>
        public List<TokenHolder> InputTokens { get; set; }

        /// <summary>Tokens emitted by the current action.</summary>
        public List<TokenHolder> OutputTokens { get; set; }
    }
}
