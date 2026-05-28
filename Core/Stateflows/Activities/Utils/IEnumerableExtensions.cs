using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Utils
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> sequence, int size)
        {
            List<T> partition = new List<T>(size);
            foreach (var item in sequence)
            {
                partition.Add(item);
                if (partition.Count == size)
                {
                    yield return partition;
                    partition = new List<T>(size);
                }
            }

            if (partition.Count > 0)
            {
                yield return partition;
            }
        }

        internal static IEnumerable<TokenHolder<T>> ToTokenHolders<T>(this IEnumerable<T> source)
            => source.Select(t => new TokenHolder<T>() { Payload = t });

        internal static IEnumerable<T> ToTokens<T>(this IEnumerable<TokenHolder<T>> source)
            => source.Select(t => t.Payload);

        internal static Type GetEffectivePayloadType(this TokenHolder token)
            => token.BoxedPayload?.GetType() ?? token.PayloadType;

        internal static bool IsOfTokenType<TToken>(this TokenHolder token)
            => typeof(TToken).IsAssignableFrom(token.GetEffectivePayloadType());

        internal static bool IsOfTokenType(this TokenHolder token, Type tokenType)
            => tokenType.IsAssignableFrom(token.GetEffectivePayloadType());

        internal static IEnumerable<TokenHolder> OfTokenType<TToken>(this IEnumerable<TokenHolder> source)
            => source.Where(token => token.IsOfTokenType<TToken>());

        internal static IEnumerable<TokenHolder> OfTokenType(this IEnumerable<TokenHolder> source, Type tokenType)
            => source.Where(token => token.IsOfTokenType(tokenType));

        internal static IEnumerable<TToken> ToTokensOfType<TToken>(this IEnumerable<TokenHolder> source)
            => source
                .OfTokenType<TToken>()
                .Select(token => (TToken)token.BoxedPayload);

        internal static IEnumerable<object> ToBoxedTokens(this IEnumerable<TokenHolder> source)
            => source.Select(t => t.BoxedPayload);
    }
}
