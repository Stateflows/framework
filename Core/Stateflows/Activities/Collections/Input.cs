using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace Stateflows.Activities
{
    public static class InputTokens
    {
        internal static readonly AsyncLocal<List<TokenHolder>> TokensHolder = new AsyncLocal<List<TokenHolder>>();

        internal static List<TokenHolder> Tokens
            => TokensHolder.Value ??= new List<TokenHolder>();

        public static void Add<TToken>(TToken token)
            => Tokens.Add(new TokenHolder<TToken>() { Payload = token });

        public static void AddRange<TToken>(IEnumerable<TToken> tokens)
            => Tokens.AddRange(tokens.Select(token => new TokenHolder<TToken>() { Payload = token }));
    }

    public struct Input<TToken> : IEnumerable<TToken>
    {
        private readonly IEnumerable<TToken> Tokens
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload);

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
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload).First();

        public readonly void PassOn()
            => new Output<TToken>().Add(Token);
    }

    public struct OptionalInput<TToken> : IEnumerable<TToken>
    {
        private readonly IEnumerable<TToken> Tokens
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload);

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
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload).First();

        public readonly void PassOn()
            => new Output<TToken>().Add(Token);
    }
}
