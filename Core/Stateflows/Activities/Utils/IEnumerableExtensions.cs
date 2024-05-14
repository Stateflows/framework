﻿using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities;

namespace Stateflows.Utils
{
    public static class IEnumerableExtensions
    {
        //public static IEnumerable<TTarget> Where<TSource, TTarget>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        //    where TTarget : TSource
        //{
        //    return source.Where(i => i is TTarget && predicate(i)) as IEnumerable<TTarget>;
        //}

        //public static TTarget First<TSource, TTarget>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        //    where TTarget : TSource
        //{
        //    return (TTarget)source.First(i => i is TTarget && predicate(i));
        //}

        //public static TTarget First<TSource, TTarget>(this IEnumerable<TSource> source)
        //    where TTarget : TSource
        //{
        //    return (TTarget)source.First(i => i is TTarget);
        //}

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

        internal static IEnumerable<Token<T>> ToTokens<T>(this IEnumerable<T> source)
            => source.Select(t => new Token<T>() { Payload = t });

        internal static IEnumerable<T> FromTokens<T>(this IEnumerable<Token<T>> source)
            => source.Select(t => t.Payload);

        internal static IEnumerable<object> FromTokens(this IEnumerable<Token> source)
            => source.Select(t => t.BoxedPayload);
    }
}
