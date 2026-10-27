using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common;

/// <summary>Adds token payloads to the input token set of a behavior.</summary>
public interface ITokensInput
{
    /// <summary>Adds a token payload.</summary>
    /// <typeparam name="TToken">The token type.</typeparam>
    /// <param name="token">The token to add.</param>
    /// <returns>This input request for fluent construction.</returns>
    ITokensInput Add<TToken>(TToken token);
    /// <summary>Adds multiple token payloads of the same type.</summary>
    /// <typeparam name="TToken">The token type.</typeparam>
    /// <param name="tokens">Tokens to add.</param>
    /// <returns>This input request for fluent construction.</returns>
    ITokensInput AddRange<TToken>(params TToken[] tokens);
}

/// <summary>Reads output token payloads of any type assignable to a requested type.</summary>
public interface ITokensOutput
{
    /// <summary>Gets all output tokens assignable to <typeparamref name="TToken"/>.</summary>
    /// <typeparam name="TToken">The token type to retrieve.</typeparam>
    /// <returns>Matching token payloads.</returns>
    IEnumerable<TToken> GetAllOfType<TToken>();
}

/// <summary>Reads output tokens of a single requested type.</summary>
/// <typeparam name="TToken">The requested token type.</typeparam>
public interface ITokensOutput<out TToken>
{
    /// <summary>Gets all output tokens of the specified type.</summary>
    /// <returns>Token payloads matching <typeparamref name="TToken"/>.</returns>
    IEnumerable<TToken> GetAll();
}

/// <summary>Base event payload that carries token envelopes between behaviors.</summary>
public class TokensTransferEvent
{
    /// <summary>Gets or sets the token envelopes carried by this event.</summary>
    [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
    public List<TokenHolder> Tokens { get; set; } = [];
}

/// <summary>Input event that inserts tokens into a behavior.</summary>
public class TokensInputEvent : TokensTransferEvent;

/// <summary>Output event that returns tokens from a behavior.</summary>
public class TokensOutputEvent : TokensTransferEvent;

/// <summary>Request for inserting token payloads of any type into a behavior.</summary>
public sealed class TokensInput : TokensInputEvent, ITokensInput, IRequest<TokensOutput>
{
    ITokensInput ITokensInput.Add<TToken>(TToken token)
        => Add(token);

    /// <summary>Adds a token payload to this input request.</summary>
    /// <typeparam name="TToken">The token type.</typeparam>
    /// <param name="token">The token to add.</param>
    /// <returns>This request for fluent construction.</returns>
    public TokensInput Add<TToken>(TToken token)
    {
        Tokens.Add(new TokenHolder<TToken>() { Payload = token });

        return this;
    }

    ITokensInput ITokensInput.AddRange<TToken>(params TToken[] tokens)
        => AddRange(tokens);

    /// <summary>Adds multiple token payloads of the same type to this input request.</summary>
    /// <typeparam name="TToken">The token type.</typeparam>
    /// <param name="tokens">Tokens to add.</param>
    /// <returns>This request for fluent construction.</returns>
    public TokensInput AddRange<TToken>(params TToken[] tokens)
    {
        Tokens.AddRange(tokens.Select(token => new TokenHolder<TToken>() { Payload = token }));

        return this;
    }
}

/// <summary>Response containing output tokens of any type.</summary>
public sealed class TokensOutput : TokensOutputEvent, ITokensOutput
{
    IEnumerable<TToken> ITokensOutput.GetAllOfType<TToken>()
        => GetOfType<TToken>();

    /// <summary>Gets output token payloads assignable to the requested type.</summary>
    /// <typeparam name="TToken">The token type to retrieve.</typeparam>
    /// <returns>Matching token payloads.</returns>
    public IEnumerable<TToken> GetOfType<TToken>()
        => Tokens
            .Where(holder => typeof(TToken).IsAssignableFrom(holder.ActualPayloadType))
            .Select(holder => (TToken)holder.BoxedPayload);
}

/// <summary>Request for inserting tokens of a single type into a behavior.</summary>
/// <typeparam name="TToken">The token type accepted by the request.</typeparam>
public sealed class TokensInput<TToken> : TokensInputEvent, IRequest<TokensOutput>
{
    /// <summary>Adds a token payload to this input request.</summary>
    /// <param name="token">The token to add.</param>
    /// <returns>This request for fluent construction.</returns>
    public TokensInput<TToken> Add(TToken token)
    {
        Tokens.Add(new TokenHolder<TToken>() { Payload = token });

        return this;
    }

    /// <summary>Adds multiple token payloads to this input request.</summary>
    /// <param name="tokens">Tokens to add.</param>
    /// <returns>This request for fluent construction.</returns>
    public TokensInput<TToken> AddRange(params TToken[] tokens)
    {
        Tokens.AddRange(tokens.Select(token => new TokenHolder<TToken>() { Payload = token }));

        return this;
    }
}

/// <summary>Response containing output tokens of a single type.</summary>
/// <typeparam name="TToken">The returned token type.</typeparam>
public sealed class TokensOutput<TToken> : TokensOutputEvent, ITokensOutput<TToken>
{
    IEnumerable<TToken> ITokensOutput<TToken>.GetAll()
        => GetAll();

    /// <summary>Gets output token payloads of the requested type.</summary>
    /// <returns>Matching token payloads.</returns>
    public IEnumerable<TToken> GetAll()
        => Tokens
            .Where(holder => typeof(TToken).IsAssignableFrom(holder.ActualPayloadType))
            .Select(holder => (TToken)holder.BoxedPayload);
}
