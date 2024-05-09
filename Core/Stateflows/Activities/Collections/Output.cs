using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Stateflows.Utils;
using Stateflows.Common;
using Stateflows.Common.Data;

namespace Stateflows.Activities
{
    internal static class OutputTokensHolder
    {
        public static readonly AsyncLocal<List<Token>> Tokens = new AsyncLocal<List<Token>>();
    }

    public struct Output<TToken> : ICollection<TToken>
    {
        private readonly List<TToken> GetTokens()
            => OutputTokensHolder.Tokens.Value.OfType<Token<TToken>>().FromTokens().ToList();

        public readonly int Count => GetTokens().Count;

        public readonly bool IsReadOnly => false;

        public readonly void Add(TToken item)
            => OutputTokensHolder.Tokens.Value.Add(item.ToToken());

        public readonly void AddRange(IEnumerable<TToken> items)
        {
            foreach (var item in items)
            {
                OutputTokensHolder.Tokens.Value.Add(item.ToToken());
            }
        }

        public readonly void Clear()
        => OutputTokensHolder.Tokens.Value.RemoveAll(token => token is TToken);

        public readonly bool Contains(TToken item)
            => OutputTokensHolder.Tokens.Value.Contains(item.ToToken());

        public readonly void CopyTo(TToken[] array, int arrayIndex)
            => OutputTokensHolder.Tokens.Value.CopyTo(array.ToTokens().ToArray(), arrayIndex);

        public readonly bool Remove(TToken item)
            => OutputTokensHolder.Tokens.Value.Remove(item.ToToken());

        public readonly IEnumerator<TToken> GetEnumerator()
            => GetTokens().GetEnumerator();

        readonly IEnumerator IEnumerable.GetEnumerator()
            => GetTokens().GetEnumerator();
    }
}
