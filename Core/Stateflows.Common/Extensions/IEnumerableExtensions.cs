using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        public static async Task RunProtected<T>(this IEnumerable<T> enumerable, ActionAsync<T> action, Action<Exception> exceptionHandler)
            where T : class
        {
            foreach (var item in enumerable)
            {
                try
                {
                    await action(item);
                }
                catch (Exception e)
                {
                    exceptionHandler(e);
                }
            }
        }

        public static async Task<bool> RunProtected<T>(this IEnumerable<T> enumerable, PredicateAsync<T> action, Action<Exception> exceptionHandler)
            where T : class
        {
            foreach (var item in enumerable)
            {
                try
                {
                    if (!await action(item))
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    exceptionHandler(e);
                }
            }

            return true;
        }

        public static Task RunSafe<T>(this IEnumerable<T> enumerable, ActionAsync<T> action, string methodName)
            where T : class
            => enumerable.RunProtected<T>(
                action,
                e => Debug.WriteLine($"Exception catched in {typeof(T).DeclaringType.Name}.{methodName}(): '{e.GetType().Name}' with message \"{e.Message}\"")
            );

        public static Task<bool> RunSafe<T>(this IEnumerable<T> enumerable, PredicateAsync<T> action, string methodName)
            where T : class
            => enumerable.RunProtected<T>(
                action,
                e => Debug.WriteLine($"Exception catched in {typeof(T).DeclaringType.Name}.{methodName}(): '{e.GetType().Name}' with message \"{e.Message}\"")
            );
    }
}
