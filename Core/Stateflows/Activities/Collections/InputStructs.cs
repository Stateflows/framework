using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Stateflows.Activities
{
    [Obsolete("Input<TToken> struct is obsolete, use IInputTokens<TToken> with dependency injection instead")]
    public struct Input<TToken> : IEnumerable<TToken>
    {
        private IEnumerable<TToken> Tokens
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload);

        public IEnumerator<TToken> GetEnumerator()
            => Tokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Tokens.GetEnumerator();

        public void PassAllOn()
            => new Output<TToken>().AddRange(Tokens);
    }

    [Obsolete("SingleInput<TToken> struct is obsolete, use IInputToken<TToken> with dependency injection instead")]
    public struct SingleInput<TToken>
    {
        public TToken Token
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload).First();

        public void PassOn()
            => new Output<TToken>().Add(Token);
    }

    [Obsolete("OptionalInput<TToken> struct is obsolete, use IOptionalInputTokens<TToken> with dependency injection instead")]
    public struct OptionalInput<TToken> : IEnumerable<TToken>
    {
        private IEnumerable<TToken> Tokens
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload);

        public IEnumerator<TToken> GetEnumerator()
            => Tokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Tokens.GetEnumerator();

        public void PassAllOn()
            => new Output<TToken>().AddRange(Tokens);
    }

    [Obsolete("OptionalSingleInput<TToken> struct is obsolete, use IOptionalInputToken<TToken> with dependency injection instead")]
    public struct OptionalSingleInput<TToken>
    {
        public bool IsAvailable
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Any();
        
        public TToken Token
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload).FirstOrDefault();

        public void PassOn()
            => new Output<TToken>().Add(Token);
    }
}
