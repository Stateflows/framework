namespace Stateflows.Activities;

/// <summary>Combines access to input tokens with the ability to forward all of them as output.</summary>
/// <typeparam name="TToken">The token payload type.</typeparam>
/// <remarks>Forwarding stages the selected input tokens as outputs for the current activity execution; it does not consume them from the input.</remarks>
public interface IInputOutputTokens<TToken> : IInputTokens<TToken>, IOutputTokens<TToken>
{
    /// <summary>Forwards every input token of this type to the activity output.</summary>
    void PassAllOn();
}

/// <summary>Combines access to one required input token with the ability to forward it as output.</summary>
/// <typeparam name="TToken">The token payload type.</typeparam>
public interface IInputOutputToken<TToken> : IInputToken<TToken>, IOutputTokens<TToken>
{
    /// <summary>Forwards the required input token to the activity output.</summary>
    void PassOn();
}

/// <summary>Combines optional access to input tokens with the ability to forward all available tokens.</summary>
/// <typeparam name="TToken">The token payload type.</typeparam>
public interface IOptionalInputOutputTokens<TToken> : IOptionalInputTokens<TToken>
{
    /// <summary>Forwards every available input token of this type to the activity output.</summary>
    void PassAllOn();
}

/// <summary>Combines optional access to one input token with the ability to forward it as output.</summary>
/// <typeparam name="TToken">The token payload type.</typeparam>
public interface IOptionalInputOutputToken<TToken> : IOptionalInputToken<TToken>
{
    /// <summary>Forwards the available input token to the activity output.</summary>
    /// <remarks>If no token is available, the default value of <typeparamref name="TToken"/> is forwarded.</remarks>
    void PassOn();
}