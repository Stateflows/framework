using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities
{
    internal static class InputTokensHolder
    {
        public static readonly AsyncLocal<IEnumerable<object>> Tokens = new AsyncLocal<IEnumerable<object>>();
    }

    public struct Input<TToken> : IEnumerable<TToken>
    {
        private readonly IEnumerable<TToken> Tokens
            => InputTokensHolder.Tokens.Value.OfType<TToken>();

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
            => InputTokensHolder.Tokens.Value.OfType<TToken>().First();

        public readonly void PassOn()
            => new Output<TToken>().Add(Token);
    }

    public struct OptionalInput<TToken> : IEnumerable<TToken>
    {
        private readonly IEnumerable<TToken> Tokens
            => InputTokensHolder.Tokens.Value.OfType<TToken>();

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
            => InputTokensHolder.Tokens.Value.OfType<TToken>().First();

        public readonly void PassOn()
            => new Output<TToken>().Add(Token);
    }
}
