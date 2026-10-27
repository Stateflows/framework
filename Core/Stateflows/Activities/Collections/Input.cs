using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Utils;

namespace Stateflows.Activities;

/// <summary>Supplies typed input tokens to the current asynchronous activity execution.</summary>
/// <remarks>Tokens added through this class are ambient to the current execution flow and are matched by payload type.</remarks>
public static class InputTokens
{
    internal static readonly AsyncLocal<List<TokenHolder>> TokensHolder = new();

    internal static List<TokenHolder> Tokens
        => TokensHolder.Value ??= new List<TokenHolder>();

    /// <summary>Adds an input token for the current asynchronous activity execution.</summary>
    /// <typeparam name="TToken">The token payload type.</typeparam>
    /// <param name="token">The token payload to supply.</param>
    /// <remarks>Input tokens are ambient execution data consumed by the activity engine. Call this only in the intended execution flow.</remarks>
    public static void Add<TToken>(TToken token)
        => Tokens.Add(new TokenHolder<TToken>() { Payload = token });

    /// <summary>Adds a sequence of input tokens for the current asynchronous activity execution.</summary>
    /// <typeparam name="TToken">The token payload type.</typeparam>
    /// <param name="tokens">The token payloads to supply.</param>
    /// <remarks>Tokens are added in enumeration order and become available to activity actions and guards in the current flow.</remarks>
    public static void AddRange<TToken>(IEnumerable<TToken> tokens)
        => Tokens.AddRange(tokens.Select(token => new TokenHolder<TToken>() { Payload = token }));
}

internal class InputTokens<TToken> : IInputTokens<TToken>
{
    internal IEnumerable<TToken> Tokens
        => InputTokens.Tokens.ToTokensOfType<TToken>();
    
    public IEnumerator<TToken> GetEnumerator()
        => Tokens.GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator()
        => Tokens.GetEnumerator();
    
    public void PassAllOn()
        => new OutputTokens<TToken>().AddRange(Tokens);
}

internal class InputToken<TToken> : IInputToken<TToken>
{
    public TToken Token
        => InputTokens.Tokens.ToTokensOfType<TToken>().First();

    public void PassOn()
        => new OutputTokens<TToken>().Add(Token);
}

internal class OptionalInputTokens<TToken> : IOptionalInputTokens<TToken>
{
    internal IEnumerable<TToken> Tokens
        => InputTokens.Tokens.ToTokensOfType<TToken>();

    public IEnumerator<TToken> GetEnumerator()
        => Tokens.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => Tokens.GetEnumerator();

    public void PassAllOn()
        => new OutputTokens<TToken>().AddRange(Tokens);
}

internal class OptionalInputToken<TToken> : IOptionalInputToken<TToken>
{
    public bool IsAvailable
        => InputTokens.Tokens.OfTokenType<TToken>().Any();
    
    public TToken TokenOrDefault
        => InputTokens.Tokens.ToTokensOfType<TToken>().FirstOrDefault();

    public void PassOn()
        => new OutputTokens<TToken>().Add(TokenOrDefault);
}