using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
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

        public static async Task<bool> RunProtected<T>(this IEnumerable<T> enumerable, PredicateAsync<T> action, Action<Exception> exceptionHandler, bool defaultResult = true)
            where T : class
        {
            var result = defaultResult;
            foreach (var item in enumerable)
            {
                try
                {
                    if (!await action(item))
                    {
                        result = false;
                    }
                }
                catch (Exception e)
                {
                    exceptionHandler(e);
                }
            }

            return result;
        }

        public static Task RunSafe<T>(this IEnumerable<T> enumerable, ActionAsync<T> action, string methodName, ILogger logger)
            where T : class
            => enumerable.RunProtected<T>(
                action,
                e => logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(T).DeclaringType.FullName, methodName, e.GetType().Name, e.Message)
            );

        public static Task<bool> RunSafe<T>(this IEnumerable<T> enumerable, PredicateAsync<T> action, string methodName, ILogger logger, bool defaultResult = true)
            where T : class
            => enumerable.RunProtected<T>(
                action,
                e => logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(T).DeclaringType.FullName, methodName, e.GetType().Name, e.Message),
                defaultResult
            );
    }
}
