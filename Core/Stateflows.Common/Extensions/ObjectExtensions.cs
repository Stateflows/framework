using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Stateflows.Activities;

namespace Stateflows.Common.Utilities
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

        [DebuggerHidden]
        public static EventHolder<TEvent> ToEventHolder<TEvent>(this TEvent payload, BehaviorId? senderId = null)
           => new EventHolder<TEvent>()
           {
               Payload = payload,
               SenderId = senderId,
           };

        [DebuggerHidden]
        public static EventHolder<TEvent> ToEventHolder<TEvent>(this TEvent payload, IDictionary<string, EventHeader> headers, BehaviorId? senderId = null)
        {
            var holder = new EventHolder<TEvent>()
            {
                Payload = payload,
                Headers = headers?.ToDictionary() ?? new Dictionary<string, EventHeader>(),
                SenderId = senderId,
            };
            ApplyEventId(holder);
            return holder;
        }

        [DebuggerHidden]
        public static EventHolder ToTypedEventHolder<TEvent>(this TEvent payload, BehaviorId? senderId = null)
        {
            if (payload is EventHolder envelope)
                return envelope;

            var eventType = payload.GetType();
            var holderType = typeof(EventHolder<>).MakeGenericType(eventType);
            var holder = (EventHolder)Activator.CreateInstance(holderType);
            holderType.GetProperty("Payload")?.SetValue(holder, payload);
            holder.SenderId = senderId;

            return holder;
        }

        [DebuggerHidden]
        public static EventHolder ToTypedEventHolder<TEvent>(this TEvent payload, IDictionary<string, EventHeader> headers, BehaviorId? senderId = null)
        {
            var holder = payload.ToTypedEventHolder(senderId);
            if (payload is EventHolder)
            {
                if (headers == null) return holder;
                holder = StateflowsJsonConverter.Clone(holder);
                foreach (var header in headers) holder.Headers[header.Key] = header.Value;
                return holder;
            }
            holder.Headers = headers?.ToDictionary() ?? new Dictionary<string, EventHeader>();
            ApplyEventId(holder);

            return holder;
        }

        private static void ApplyEventId(EventHolder holder)
        {
            var eventId = holder.Headers.Values.OfType<EventId>().SingleOrDefault();
            if (eventId != null)
                holder.Id = eventId.Id;
        }

        [DebuggerHidden]
        public static TokenHolder<TToken> ToTokenHolder<TToken>(this TToken payload)
           => new TokenHolder<TToken>() { Payload = payload };

        [DebuggerHidden]
        public static TokenHolder ToTokenHolder<TToken>(this TToken payload, Type tokenType)
        {
            var holderType = typeof(TokenHolder<>).MakeGenericType(tokenType);
            var holder = (TokenHolder)Activator.CreateInstance(holderType);
            holderType.GetProperty("Payload")?.SetValue(holder, payload);

            return holder;
        }
    }
}
