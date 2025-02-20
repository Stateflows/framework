using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Stateflows.Utils;
using Stateflows.Common;

namespace Stateflows.Activities
{
    [Obsolete("Output<TToken> struct is obsolete, use IOutputTokens<TToken> with dependency injection instead")]
    public struct Output<TToken> : ICollection<TToken>
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
