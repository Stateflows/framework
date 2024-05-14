using System;
using System.Linq;
using System.Reflection;
using Stateflows.Activities;

namespace Stateflows.Utils
{
    internal static class ObjectExtensions
    {
        internal static TokenHolder<T> ToToken<T>(this T payload)
           => new TokenHolder<T>() { Payload = payload };

        internal static TokenHolder ToToken<T>(this T payload, Type tokenType)
        {
            var holderType = typeof(TokenHolder<>).MakeGenericType(tokenType);
            var holder = (TokenHolder)Activator.CreateInstance(holderType);
            holderType.GetProperty("Payload").SetValue(holder, payload);

            return holder;
        }
    }
}
