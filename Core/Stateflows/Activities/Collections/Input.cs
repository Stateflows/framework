using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Stateflows.Activities.Collections
{
    internal static class InputTokensHolder
    {
        public static readonly AsyncLocal<IEnumerable<Token>> Tokens = new AsyncLocal<IEnumerable<Token>>();
    }

    public struct Input<TToken> : IEnumerable<TToken>
        where TToken : Token, new()
    {
        private readonly IEnumerable<TToken> Tokens
            => InputTokensHolder.Tokens.Value.OfType<TToken>();

        public IEnumerator<TToken> GetEnumerator()
            => Tokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Tokens.GetEnumerator();
    }

    public struct OptionalInput<TToken> : IEnumerable<TToken>
        where TToken : Token, new()
    {
        private readonly IEnumerable<TToken> Tokens
            => InputTokensHolder.Tokens.Value.OfType<TToken>();

        public IEnumerator<TToken> GetEnumerator()
            => Tokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Tokens.GetEnumerator();
    }
}
