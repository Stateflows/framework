using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Stateflows.Utils;

namespace Stateflows.Activities
{
    public static class OutputTokens
    {
        internal static readonly AsyncLocal<List<TokenHolder>> TokensHolder = new AsyncLocal<List<TokenHolder>>();

        internal static List<TokenHolder> Tokens
            => TokensHolder.Value ??= new List<TokenHolder>();

        public static List<TToken> GetAllOfType<TToken>()
            => Tokens.OfType<TokenHolder<TToken>>().ToTokens().ToList();

        public static List<object> GetAll()
            => Tokens.ToBoxedTokens().ToList();

        public static int Count => Tokens.Count;

        public static bool Contains<TToken>(TToken item)
            => Tokens.Contains(item.ToTokenHolder());
    }

    public struct Output<TToken> : ICollection<TToken>
    {
        private readonly List<TToken> GetTokens()
            => OutputTokens.Tokens.OfType<TokenHolder<TToken>>().ToTokens().ToList();

        public readonly int Count => GetTokens().Count;

        public readonly bool IsReadOnly => false;

        public readonly void Add(TToken item)
            => OutputTokens.Tokens.Add(item.ToTokenHolder());

        public readonly void AddRange(IEnumerable<TToken> items)
            => OutputTokens.Tokens.AddRange(items.Select(item => item.ToTokenHolder()));

        public readonly void Clear()
        => OutputTokens.Tokens.RemoveAll(token => token is TToken);

        public readonly bool Contains(TToken item)
            => OutputTokens.Tokens.Contains(item.ToTokenHolder());

        public readonly void CopyTo(TToken[] array, int arrayIndex)
            => OutputTokens.Tokens.CopyTo(array.ToTokenHolders().ToArray(), arrayIndex);

        public readonly bool Remove(TToken item)
            => OutputTokens.Tokens.Remove(item.ToTokenHolder());

        public readonly IEnumerator<TToken> GetEnumerator()
            => GetTokens().GetEnumerator();

        readonly IEnumerator IEnumerable.GetEnumerator()
            => GetTokens().GetEnumerator();
    }
}
