using System.Collections.Generic;

namespace Stateflows.Activities
{
    /// <summary>Provides the input tokens of a requested type to an activity action or guard.</summary>
    /// <typeparam name="TToken">The token payload type to enumerate.</typeparam>
    /// <remarks>Input tokens are supplied for the current activity execution; forwarding them does not remove them from the input collection.</remarks>
    public interface IInputTokens<out TToken> : IEnumerable<TToken>
    { }

    /// <summary>Provides one required input token of a requested type.</summary>
    /// <typeparam name="TToken">The token payload type.</typeparam>
    /// <remarks>Use this contract when the activity definition requires a matching input token.</remarks>
    public interface IInputToken<out TToken>
    {
        /// <summary>Gets the matching input token.</summary>
        /// <remarks>Access requires a matching token to be available; otherwise the underlying lookup fails.</remarks>
        TToken Token { get; }
    }

    /// <summary>Provides zero or more input tokens of a requested type.</summary>
    /// <typeparam name="TToken">The token payload type to enumerate.</typeparam>
    /// <remarks>Unlike <see cref="IInputTokens{TToken}"/>, this contract allows no matching input token to be present.</remarks>
    public interface IOptionalInputTokens<out TToken> : IEnumerable<TToken>
    { }

    /// <summary>Provides optional access to one input token of a requested type.</summary>
    /// <typeparam name="TToken">The token payload type.</typeparam>
    public interface IOptionalInputToken<out TToken>
    {
        /// <summary>Gets whether a matching input token is available.</summary>
        bool IsAvailable { get; }
        
        /// <summary>Gets the matching token, or the default value of <typeparamref name="TToken"/> when none is available.</summary>
        TToken TokenOrDefault { get; }
    }
}
