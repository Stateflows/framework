using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common;

public interface ITokensInput
{
    ITokensInput Add<TToken>(TToken token);
    ITokensInput AddRange<TToken>(params TToken[] tokens);
}

public interface ITokensOutput
{
    IEnumerable<TToken> GetAllOfType<TToken>();
}

public interface ITokensOutput<out TToken>
{
    IEnumerable<TToken> GetAll();
}

public class TokensTransferEvent
{
    [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
    public List<TokenHolder> Tokens { get; set; } = [];
}

public class TokensInputEvent : TokensTransferEvent;

public class TokensOutputEvent : TokensTransferEvent;

public sealed class TokensInput : TokensInputEvent, ITokensInput, IRequest<TokensOutput>
{
    ITokensInput ITokensInput.Add<TToken>(TToken token)
        => Add(token);

    public TokensInput Add<TToken>(TToken token)
    {
        Tokens.Add(new TokenHolder<TToken>() { Payload = token });

        return this;
    }

    ITokensInput ITokensInput.AddRange<TToken>(params TToken[] tokens)
        => AddRange(tokens);

    public TokensInput AddRange<TToken>(params TToken[] tokens)
    {
        Tokens.AddRange(tokens.Select(token => new TokenHolder<TToken>() { Payload = token }));

        return this;
    }
}

public sealed class TokensOutput : TokensOutputEvent, ITokensOutput
{
    IEnumerable<TToken> ITokensOutput.GetAllOfType<TToken>()
        => GetOfType<TToken>();

    public IEnumerable<TToken> GetOfType<TToken>()
        => Tokens
            .Where(holder => typeof(TToken).IsAssignableFrom(holder.BoxedPayload?.GetType() ?? holder.PayloadType))
            .Select(holder => (TToken)holder.BoxedPayload);
}

public sealed class TokensInput<TToken> : TokensInputEvent, IRequest<TokensOutput>
{
    public TokensInput<TToken> Add(TToken token)
    {
        Tokens.Add(new TokenHolder<TToken>() { Payload = token });

        return this;
    }

    public TokensInput<TToken> AddRange(params TToken[] tokens)
    {
        Tokens.AddRange(tokens.Select(token => new TokenHolder<TToken>() { Payload = token }));

        return this;
    }
}

public sealed class TokensOutput<TToken> : TokensOutputEvent, ITokensOutput<TToken>
{
    IEnumerable<TToken> ITokensOutput<TToken>.GetAll()
        => GetAll();

    public IEnumerable<TToken> GetAll()
        => Tokens
            .Where(holder => typeof(TToken).IsAssignableFrom(holder.BoxedPayload?.GetType() ?? holder.PayloadType))
            .Select(holder => (TToken)holder.BoxedPayload);
}
