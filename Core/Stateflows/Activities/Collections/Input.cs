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

    internal class InputTokens<TToken> : IInputTokens<TToken>
    {
        private IEnumerable<TToken> Tokens
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload);
        
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
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload).First();

        public void PassOn()
            => new OutputTokens<TToken>().Add(Token);
    }

    internal class OptionalInputTokens<TToken> : IOptionalInputTokens<TToken>
    {
        private IEnumerable<TToken> Tokens
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload);

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
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Any();
        
        public TToken TokenOrDefault
            => InputTokens.Tokens.OfType<TokenHolder<TToken>>().Select(t => t.Payload).FirstOrDefault();

        public void PassOn()
            => new OutputTokens<TToken>().Add(TokenOrDefault);
    }
}
