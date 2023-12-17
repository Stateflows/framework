using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Stateflows.Activities.Collections
{
    internal static class OutputTokensHolder
    {
        public static readonly AsyncLocal<List<Token>> Tokens = new AsyncLocal<List<Token>>();
    }

    public struct Output<TToken> : ICollection<TToken>
        where TToken : Token, new()
    {
        private List<TToken> GetTokens()
            => OutputTokensHolder.Tokens.Value.OfType<TToken>().ToList();

        public int Count => GetTokens().Count;

        public bool IsReadOnly => false;

        public void Add(TToken item)
            => OutputTokensHolder.Tokens.Value.Add(item);

        public void AddRange(IEnumerable<TToken> items)
        {
            foreach (var item in items)
            {
                OutputTokensHolder.Tokens.Value.Add(item);
            }
        }

        public void Clear()
        => OutputTokensHolder.Tokens.Value.RemoveAll(token => token is TToken);

        public bool Contains(TToken item)
            => OutputTokensHolder.Tokens.Value.Contains(item);

        public void CopyTo(TToken[] array, int arrayIndex)
            => OutputTokensHolder.Tokens.Value.CopyTo(array, arrayIndex);

        public bool Remove(TToken item)
            => OutputTokensHolder.Tokens.Value.Remove(item);

        public IEnumerator<TToken> GetEnumerator()
            => GetTokens().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetTokens().GetEnumerator();
    }
}
