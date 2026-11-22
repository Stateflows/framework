using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Provides access to tokens available as input to an activity operation.</summary>
    public interface IInput
    {
        /// <summary>Gets available input tokens of the requested type.</summary>
        /// <typeparam name="TToken">The token type to retrieve.</typeparam>
        /// <returns>The matching tokens; an empty sequence is returned when none are available.</returns>
        IEnumerable<TToken> GetTokensOfType<TToken>();
        
        /// <summary>Determines whether any input token of the requested type is available.</summary>
        /// <typeparam name="TToken">The token type to check.</typeparam>
        /// <returns><see langword="true"/> if at least one matching token is available; otherwise, <see langword="false"/>.</returns>
        bool HasTokensOfType<TToken>();

        /// <summary>Gets available payload tokens with the specified name.</summary>
        IEnumerable<Stateflows.Common.Payload> GetPayloadTokens(string tokenName);

        /// <summary>Gets available payload tokens whose names are in the specified set.</summary>
        IEnumerable<Stateflows.Common.Payload> GetPayloadTokens(IReadOnlyList<string> tokenNames);

        /// <summary>Determines whether an input payload token with the specified name is available.</summary>
        bool HasPayloadTokens(string tokenName);

        /// <summary>Determines whether any input payload token with the specified name is available.</summary>
        bool HasPayloadTokens(IReadOnlyList<string> tokenNames);
    }
}
