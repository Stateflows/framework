using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Stateflows.Activities;

namespace Stateflows.Common.Utilities
{
    /// <summary>Creates delivery envelopes from payloads and provides argument-validation helpers.</summary>
    public static class ObjectExtensions
    {
        /// <summary>Throws when a required value is null.</summary>
        /// <param name="value">The value to inspect.</param>
        /// <param name="name">The argument name reported by the exception.</param>
        /// <exception cref="ArgumentNullException">The value is null.</exception>
        public static void ThrowIfNull(this object value, string name)
            => _ = value ?? throw new ArgumentNullException(name);

        /// <summary>Throws when a required string is null or empty.</summary>
        /// <param name="value">The string to inspect.</param>
        /// <param name="name">The argument name reported by the exception.</param>
        /// <exception cref="ArgumentNullException">The string is null or empty.</exception>
        public static void ThrowIfNullOrEmpty(this string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>Wraps a payload in an envelope retaining its declared type.</summary>
        /// <typeparam name="TEvent">The declared payload type.</typeparam>
        /// <param name="payload">The payload to wrap.</param>
        /// <param name="senderId">Optional identifier of the sender.</param>
        /// <returns>A new event envelope with a generated delivery identifier.</returns>
        [DebuggerHidden]
        public static EventHolder<TEvent> ToEventHolder<TEvent>(this TEvent payload, BehaviorId? senderId = null)
           => new EventHolder<TEvent>()
           {
               Payload = payload,
               SenderId = senderId,
           };

        /// <summary>Wraps a payload with copied delivery headers and an optional sender identifier.</summary>
        /// <typeparam name="TEvent">The declared payload type.</typeparam>
        /// <param name="payload">The payload to wrap.</param>
        /// <param name="headers">Headers copied into the envelope; an event-ID header overrides its generated identifier.</param>
        /// <param name="senderId">Optional identifier of the sender.</param>
        /// <returns>A new envelope carrying the supplied payload and header values.</returns>
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

        /// <summary>Wraps a payload using its runtime type, or returns an existing envelope unchanged.</summary>
        /// <typeparam name="TEvent">The declared input type.</typeparam>
        /// <param name="payload">The non-null payload or existing envelope.</param>
        /// <param name="senderId">The sender assigned to a newly created envelope.</param>
        /// <returns>The existing envelope, or a new envelope specialized for the runtime payload type.</returns>
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

        /// <summary>Creates a runtime-typed envelope with headers, or merges headers into a clone of an existing envelope.</summary>
        /// <typeparam name="TEvent">The declared input type.</typeparam>
        /// <param name="payload">The non-null payload or existing envelope.</param>
        /// <param name="headers">Headers copied into a new envelope or merged by key into an existing envelope's clone.</param>
        /// <param name="senderId">The sender assigned only when creating an envelope for a raw payload.</param>
        /// <returns>A new runtime-typed envelope, a clone when headers are merged, or the original envelope when headers are null.</returns>
        /// <remarks>An event-ID header sets the delivery identifier only when wrapping a raw payload, not when merging into an envelope.</remarks>
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

        /// <summary>Wraps a token payload in an envelope retaining its declared type.</summary>
        /// <typeparam name="TToken">The declared token type.</typeparam>
        /// <param name="payload">The token payload.</param>
        /// <returns>A new typed token envelope.</returns>
        [DebuggerHidden]
        public static TokenHolder<TToken> ToTokenHolder<TToken>(this TToken payload)
           => new TokenHolder<TToken>() { Payload = payload };

        /// <summary>Wraps a token payload using an explicitly supplied envelope payload type.</summary>
        /// <typeparam name="TToken">The declared input payload type.</typeparam>
        /// <param name="payload">The token payload, assignable to the supplied type.</param>
        /// <param name="tokenType">The CLR payload type used to construct the envelope.</param>
        /// <returns>A new envelope specialized for the supplied token type.</returns>
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
