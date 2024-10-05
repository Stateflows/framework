using System;
using Stateflows.Common;
using Stateflows.Activities;

namespace Stateflows.Utils
{
    public static class ObjectExtensions
    {
        public static EventHolder<TEvent> ToEventHolder<TEvent>(this TEvent payload, BehaviorId? senderId = null)
           => new EventHolder<TEvent>()
           {
               Payload = payload,
               SenderId = senderId != null
                   ? (BehaviorId)senderId
                   : default
           };

        public static EventHolder ToEventHolder<TEvent>(this TEvent payload, Type eventType, BehaviorId? senderId = null)
        {
            var holderType = typeof(EventHolder<>).MakeGenericType(eventType);
            var holder = (EventHolder)Activator.CreateInstance(holderType);
            holderType.GetProperty("Payload").SetValue(holder, payload);
            if (senderId != null)
            {
                holder.SenderId = (BehaviorId)senderId;
            }

            return holder;
        }

        internal static TokenHolder<TToken> ToTokenHolder<TToken>(this TToken payload)
           => new TokenHolder<TToken>() { Payload = payload };

        internal static TokenHolder ToTokenHolder<TToken>(this TToken payload, Type tokenType)
        {
            var holderType = typeof(TokenHolder<>).MakeGenericType(tokenType);
            var holder = (TokenHolder)Activator.CreateInstance(holderType);
            holderType.GetProperty("Payload").SetValue(holder, payload);

            return holder;
        }

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
