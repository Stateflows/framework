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
        public static void RunProtected<T>(this IEnumerable<T> enumerable, Action<T> action, Action<Exception, T> exceptionHandler)
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
                    exceptionHandler(e, item);
                }
            }
        }

        [DebuggerHidden]
        public static bool RunProtected<T>(this IEnumerable<T> enumerable, Predicate<T> action, Action<Exception, T> exceptionHandler, bool defaultResult = true)
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
                    exceptionHandler(e, item);
                }
            }

            return result;
        }

        [DebuggerHidden]
        public static void RunSafe<T>(this IEnumerable<T> enumerable, Action<T> action, string methodName, ILogger logger)
            where T : class
            => enumerable.RunProtected<T>(
                action,
                (e, t) => logger.LogError(LogTemplates.ExceptionLogTemplate, t.GetType().FullName, methodName, e.GetType().Name, e.Message)
            );

        [DebuggerHidden]
        public static bool RunSafe<T>(this IEnumerable<T> enumerable, Predicate<T> action, string methodName, ILogger logger, bool defaultResult = true)
            where T : class
            => enumerable.RunProtected<T>(
                action,
                (e, t) => logger.LogError(LogTemplates.ExceptionLogTemplate, t.GetType().FullName, methodName, e.GetType().Name, e.Message),
                defaultResult
            );
    }
}
