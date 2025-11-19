using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Utilities;
using System.Diagnostics;
using System.Linq;

namespace Stateflows.Common.Extensions
{
    public static class IEnumerableAsyncExtensions
    {
        [DebuggerHidden]
        public static async Task RunProtectedAsync<T>(this IEnumerable<T> enumerable, ActionAsync<T> action, Action<Exception> exceptionHandler)
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

        [DebuggerHidden]
        public static async Task<bool> RunProtectedAsync<T>(this IEnumerable<T> enumerable, PredicateAsync<T> action, Action<Exception> exceptionHandler, bool defaultResult = true)
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
        
        [DebuggerHidden]
        public static void WaitProtected<T>(this IEnumerable<T> enumerable, ActionAsync<T> action, Action<Exception> exceptionHandler)
            where T : class
        {
            var tasks = enumerable
                .Select(async item =>
                {
                    try
                    {
                        await action(item);
                    }
                    catch (Exception e)
                    {
                        exceptionHandler(e);
                    }
                })
                .ToArray();
            
            Task.WaitAll(tasks);
        }

        [DebuggerHidden]
        public static bool WaitProtected<T>(this IEnumerable<T> enumerable, PredicateAsync<T> action, Action<Exception> exceptionHandler, bool defaultResult = true)
            where T : class
        {
            var tasks = enumerable
                .Select(async item =>
                {
                    var result = defaultResult;
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

                    return result;
                })
                .ToArray();

            var result = defaultResult;
            Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                if (!task.Result)
                {
                    result = false;
                }
            }

            return result;
        }

        [DebuggerHidden]
        public static Task RunSafeAsync<T>(this IEnumerable<T> enumerable, ActionAsync<T> action, string methodName, ILogger logger)
            where T : class
            => enumerable.RunProtectedAsync<T>(
                action,
                e => logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(T).DeclaringType.FullName, methodName, e.GetType().Name, e.Message)
            );
        
        [DebuggerHidden]
        public static Task<bool> RunSafeAsync<T>(this IEnumerable<T> enumerable, PredicateAsync<T> action, string methodName, ILogger logger, bool defaultResult = true)
            where T : class
            => enumerable.RunProtectedAsync<T>(
                action,
                e => logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(T).DeclaringType.FullName, methodName, e.GetType().Name, e.Message),
                defaultResult
            );

        [DebuggerHidden]
        public static void RunSafe<T>(this IEnumerable<T> enumerable, ActionAsync<T> action, string methodName, ILogger logger)
            where T : class
            => enumerable.WaitProtected<T>(
                action,
                e => logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(T).DeclaringType.FullName, methodName, e.GetType().Name, e.Message)
            );

        [DebuggerHidden]
        public static bool RunSafe<T>(this IEnumerable<T> enumerable, PredicateAsync<T> action, string methodName, ILogger logger, bool defaultResult = true)
            where T : class
            => enumerable.WaitProtected<T>(
                action,
                e => logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(T).DeclaringType.FullName, methodName, e.GetType().Name, e.Message),
                defaultResult
            );
    }
}
