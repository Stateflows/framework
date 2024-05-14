using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities;

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

        internal static IEnumerable<TokenHolder<T>> ToTokens<T>(this IEnumerable<T> source)
            => source.Select(t => new TokenHolder<T>() { Payload = t });

        internal static IEnumerable<T> FromTokens<T>(this IEnumerable<TokenHolder<T>> source)
            => source.Select(t => t.Payload);

        internal static IEnumerable<object> FromTokens(this IEnumerable<TokenHolder> source)
            => source.Select(t => t.BoxedPayload);
    }
}
