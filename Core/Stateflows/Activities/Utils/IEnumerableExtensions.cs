using System;
using System.Linq;
using System.Collections.Generic;

namespace Stateflows.Activities.Utils
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TTarget> Where<TSource, TTarget>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            where TTarget : TSource
        {
            return source.Where(i => i is TTarget && predicate(i)) as IEnumerable<TTarget>;
        }

        public static TTarget First<TSource, TTarget>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            where TTarget : TSource
        {
            return (TTarget)source.First(i => i is TTarget && predicate(i));
        }

        public static TTarget First<TSource, TTarget>(this IEnumerable<TSource> source)
            where TTarget : TSource
        {
            return (TTarget)source.First(i => i is TTarget);
        }
    }
}
