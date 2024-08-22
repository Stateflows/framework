using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities;
using Stateflows.Activities.Events;

namespace Stateflows.Utils
{
    internal static class ObjectExtensions
    {
        internal static TokenHolder<T> ToTokenHolder<T>(this T payload)
           => new TokenHolder<T>() { Payload = payload };

        internal static TokenHolder ToTokenHolder<T>(this T payload, Type tokenType)
        {
            var holderType = typeof(TokenHolder<>).MakeGenericType(tokenType);
            var holder = (TokenHolder)Activator.CreateInstance(holderType);
            holderType.GetProperty("Payload").SetValue(holder, payload);

            return holder;
        }

        internal static ExceptionHolder<T> ToExceptionHolder<T>(this T payload)
            where T : Exception
           => new ExceptionHolder<T>() { Exception = payload };

        internal static TokenHolder ToExceptionHolder<T>(this T payload, Type exceptionType)
            where T : Exception
        {
            var holderType = typeof(ExceptionHolder<>).MakeGenericType(exceptionType);
            var holder = (ExceptionHolder)Activator.CreateInstance(holderType);
            holderType.GetProperty("Exception").SetValue(holder, payload);

            return holder;
        }

        public static void AddInputToken<TToken>(this ExecutionRequest executionRequest, TToken token)
            => executionRequest.InputTokens.Add(token.ToTokenHolder());

        public static void AddInputTokens<TToken>(this ExecutionRequest executionRequest, IEnumerable<TToken> tokens)
            => executionRequest.InputTokens.AddRange(tokens.Select(token => token.ToTokenHolder()));
    }
}
