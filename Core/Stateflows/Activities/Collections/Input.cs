using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace Stateflows.Activities
{
    public static class InputTokensHolder
    {
        public static readonly AsyncLocal<List<TokenHolder>> Tokens = new AsyncLocal<List<TokenHolder>>();
    }

    [Serializable]
    public struct Input<TToken> : IEnumerable<TToken>
    {
        private readonly IEnumerable<TToken> Tokens
            => InputTokensHolder.Tokens.Value.OfType<TokenHolder<TToken>>().Select(t => t.Payload);

        public readonly IEnumerator<TToken> GetEnumerator()
            => Tokens.GetEnumerator();

        readonly IEnumerator IEnumerable.GetEnumerator()
            => Tokens.GetEnumerator();

        public readonly void PassAllOn()
            => new Output<TToken>().AddRange(Tokens);
    }

    [Serializable]
    public struct SingleInput<TToken>
    {
        public readonly TToken Token
            => InputTokensHolder.Tokens.Value.OfType<TokenHolder<TToken>>().Select(t => t.Payload).First();

        public readonly void PassOn()
            => new Output<TToken>().Add(Token);
    }

    [Serializable]
    public struct OptionalInput<TToken> : IEnumerable<TToken>
    {
        private readonly IEnumerable<TToken> Tokens
            => InputTokensHolder.Tokens.Value.OfType<TokenHolder<TToken>>().Select(t => t.Payload);

        public readonly IEnumerator<TToken> GetEnumerator()
            => Tokens.GetEnumerator();

        readonly IEnumerator IEnumerable.GetEnumerator()
            => Tokens.GetEnumerator();

        public readonly void PassAllOn()
            => new Output<TToken>().AddRange(Tokens);
    }

    [Serializable]
    public struct OptionalSingleInput<TToken>
    {
        public readonly TToken Token
            => InputTokensHolder.Tokens.Value.OfType<TokenHolder<TToken>>().Select(t => t.Payload).First();

        public readonly void PassOn()
            => new Output<TToken>().Add(Token);
    }
}
