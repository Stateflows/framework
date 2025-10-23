using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Utilities;
using System.Diagnostics;

namespace Stateflows.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        [DebuggerHidden]
        public static void RunProtected<T>(this IEnumerable<T> enumerable, Action<T> action, Action<T, Exception> exceptionHandler)
            where T : class
        {
            foreach (var item in enumerable)
            {
                try
                {
                    action(item);
                }
                catch (Exception e)
                {
                    exceptionHandler(item, e);
                }
            }
        }

        [DebuggerHidden]
        public static bool RunProtected<T>(this IEnumerable<T> enumerable, Predicate<T> action, Action<T, Exception> exceptionHandler, bool defaultResult = true)
            where T : class
        {
            var result = defaultResult;
            foreach (var item in enumerable)
            {
                try
                {
                    var actualResult = action(item);
                    if (actualResult != defaultResult)
                    {
                        result = actualResult;
                    }
                }
                catch (Exception e)
                {
                    exceptionHandler(item, e);
                }
            }

            return result;
        }

        [DebuggerHidden]
        public static void RunSafe<T>(this IEnumerable<T> enumerable, Action<T> action, string methodName, ILogger logger)
            where T : class
            => enumerable.RunProtected<T>(
                action,
                (item, e) => logger.LogError(LogTemplates.ExceptionLogTemplate, item.GetType().FullName, methodName, e.GetType().Name, e.Message)
            );

        [DebuggerHidden]
        public static bool RunSafe<T>(this IEnumerable<T> enumerable, Predicate<T> action, string methodName, ILogger logger, bool defaultResult = true)
            where T : class
            => enumerable.RunProtected<T>(
                action,
                (item, e) => logger.LogError(LogTemplates.ExceptionLogTemplate, item.GetType().FullName, methodName, e.GetType().Name, e.Message),
                defaultResult
            );
    }
}
