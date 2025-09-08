using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Stateflows.Common.Utilities;
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

    internal class OutputTokens<TToken> : IOutputTokens<TToken>
    {
        private List<TToken> GetTokens()
            => OutputTokens.Tokens.OfType<TokenHolder<TToken>>().ToTokens().ToList();

        public int Count => GetTokens().Count;

        public bool IsReadOnly => false;

        public void Add(TToken item)
            => OutputTokens.Tokens.Add(item.ToTokenHolder());

        public void AddRange(IEnumerable<TToken> items)
            => OutputTokens.Tokens.AddRange(items.Select(item => item.ToTokenHolder()));

        public void Clear()
        => OutputTokens.Tokens.RemoveAll(token => token is TToken);

        public bool Contains(TToken item)
            => OutputTokens.Tokens.Contains(item.ToTokenHolder());

        public void CopyTo(TToken[] array, int arrayIndex)
            => OutputTokens.Tokens.CopyTo(array.ToTokenHolders().ToArray(), arrayIndex);

        public bool Remove(TToken item)
            => OutputTokens.Tokens.Remove(item.ToTokenHolder());

        public IEnumerator<TToken> GetEnumerator()
            => GetTokens().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetTokens().GetEnumerator();
    }
}
