using System;
using Stateflows.Activities;

namespace Stateflows.Common
{
    public static class ObjectExtensions
    {
        internal static ExceptionHolder<TException> ToExceptionHolder<TException>(this TException payload)
            where TException : Exception
           => new ExceptionHolder<TException>() { Exception = payload };

        internal static TokenHolder ToExceptionHolder<TException>(this TException payload, Type exceptionType)
            where TException : Exception
        {
            var holderType = typeof(ExceptionHolder<>).MakeGenericType(exceptionType);
            var holder = (ExceptionHolder)Activator.CreateInstance(holderType);
            holderType.GetProperty("Exception").SetValue(holder, payload);

            return holder;
        }
    }
}
