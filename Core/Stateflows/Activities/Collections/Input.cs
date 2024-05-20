using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace Stateflows.Activities
{
    internal static class InputTokensHolder
    {
        public static readonly AsyncLocal<IEnumerable<TokenHolder>> Tokens = new AsyncLocal<IEnumerable<TokenHolder>>();
    }

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

    public struct SingleInput<TToken>
    {
        public readonly TToken Token
            => InputTokensHolder.Tokens.Value.OfType<TokenHolder<TToken>>().Select(t => t.Payload).First();

        public readonly void PassOn()
            => new Output<TToken>().Add(Token);
    }

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

    public struct OptionalSingleInput<TToken>
    {
        public readonly TToken Token
            => InputTokensHolder.Tokens.Value.OfType<TokenHolder<TToken>>().Select(t => t.Payload).First();

        public readonly void PassOn()
            => new Output<TToken>().Add(Token);
    }
}
