using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities;

namespace Stateflows.Common
{
    public static class ObjectExtensions
    {
        public static void ThrowIfNull(this object value, string name)
            => _ = value ?? throw new ArgumentNullException(name);

        public static void ThrowIfNullOrEmpty(this string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(name);
            }
        }

        public static EventHolder<TEvent> ToEventHolder<TEvent>(this TEvent payload, BehaviorId? senderId = null)
           => new EventHolder<TEvent>()
           {
               Payload = payload,
               SenderId = senderId,
           };

        public static EventHolder<TEvent> ToEventHolder<TEvent>(this TEvent payload, IEnumerable<EventHeader> headers, BehaviorId? senderId = null)
           => new EventHolder<TEvent>()
           {
               Payload = payload,
               Headers = headers?.ToList() ?? new List<EventHeader>(),
               SenderId = senderId,
           };

        public static EventHolder ToTypedEventHolder<TEvent>(this TEvent payload, BehaviorId? senderId = null)
        {
            var eventType = payload.GetType();
            var holderType = typeof(EventHolder<>).MakeGenericType(eventType);
            var holder = (EventHolder)Activator.CreateInstance(holderType);
            holderType.GetProperty("Payload")?.SetValue(holder, payload);
            holder.SenderId = senderId;

            return holder;
        }

        public static EventHolder ToTypedEventHolder<TEvent>(this TEvent payload, IEnumerable<EventHeader> headers, BehaviorId? senderId = null)
        {
            var holder = payload.ToTypedEventHolder(senderId);
            holder.Headers = headers?.ToList() ?? new List<EventHeader>();

            return holder;
        }

        public static TokenHolder<TToken> ToTokenHolder<TToken>(this TToken payload)
           => new TokenHolder<TToken>() { Payload = payload };

        public static TokenHolder ToTokenHolder<TToken>(this TToken payload, Type tokenType)
        {
            var holderType = typeof(TokenHolder<>).MakeGenericType(tokenType);
            var holder = (TokenHolder)Activator.CreateInstance(holderType);
            holderType.GetProperty("Payload")?.SetValue(holder, payload);

            return holder;
        }
    }
}
