﻿using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Stateflows.Common;

namespace Stateflows.Activities.Events
{
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
        public List<TokenHolder> Tokens { get; set; } = new List<TokenHolder>();
    }

    public sealed class TokensInput : TokensTransferEvent, ITokensInput, IRequest<TokensOutput>
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

    public sealed class TokensOutput : TokensTransferEvent, ITokensOutput
    {
        IEnumerable<TToken> ITokensOutput.GetAllOfType<TToken>()
            => GetOfType<TToken>();

        public IEnumerable<TToken> GetOfType<TToken>()
            => Tokens.OfType<TokenHolder<TToken>>().Select(holder => holder.Payload);
    }

    public sealed class TokensInput<TToken> : TokensTransferEvent, IRequest<TokensOutput>
    {
        public void Add(TToken token)
            => Tokens.Add(new TokenHolder<TToken>() { Payload = token });

        public void AddRange(params TToken[] tokens)
            => Tokens.AddRange(tokens.Select(token => new TokenHolder<TToken>() { Payload = token }));
    }

    public sealed class TokensOutput<TToken> : TokensTransferEvent, ITokensOutput<TToken>
    {
        IEnumerable<TToken> ITokensOutput<TToken>.GetAll()
            => GetAll();

        public IEnumerable<TToken> GetAll()
            => Tokens.OfType<TokenHolder<TToken>>().Select(holder => holder.Payload);
    }
}