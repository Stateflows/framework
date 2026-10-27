using Stateflows.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Provides helpers for responding to and inspecting event envelopes.</summary>
    public static class EventHolderExtensions
    {
        /// <summary>Completes a request represented by an event envelope with a response envelope.</summary>
        /// <param name="eventHolder">The request event envelope.</param>
        /// <param name="response">The response event envelope.</param>
        /// <exception cref="InvalidOperationException">The event payload does not represent a request.</exception>
        public static void Respond(this EventHolder eventHolder, EventHolder response)
        {
            if (eventHolder.BoxedPayload is StructuralRequest request)
            {
                if (response.BoxedPayload is StructuralEvent structuralResponse) request.Respond(structuralResponse);
                else request.Respond(response.BoxedPayload);
                return;
            }
            if (!eventHolder.IsRequest())
            {
                throw new InvalidOperationException("Event type does not implement IRequest<> interface and cannot be responded to.");
            }

            ResponseHolder.Respond(eventHolder.BoxedPayload, response);
        }

        /// <summary>Determines whether the event payload implements a request contract.</summary>
        /// <param name="eventHolder">The event envelope to inspect.</param>
        /// <returns><see langword="true"/> when the event is a request.</returns>
        public static bool IsRequest(this EventHolder eventHolder)
            => eventHolder.PayloadType.IsRequest();

        /// <summary>Determines whether a response has been associated with this request.</summary>
        /// <param name="eventHolder">The request envelope to inspect.</param>
        /// <returns><see langword="true"/> when a response has been recorded.</returns>
        public static bool IsRespondedTo(this EventHolder eventHolder)
            => eventHolder.IsRequest() && ResponseHolder.IsResponded(eventHolder.BoxedPayload);

        /// <summary>Gets the response envelope associated with this request, if any.</summary>
        /// <param name="eventHolder">The request envelope to inspect.</param>
        /// <returns>The response envelope, or <see langword="null"/> when no response is available.</returns>
        public static EventHolder GetResponseHolder(this EventHolder eventHolder)
            => eventHolder.IsRequest()
                ? ResponseHolder.GetResponseOrDefault(eventHolder.BoxedPayload)
                : null;

        /// <summary>Gets the boxed response payload associated with this request, if any.</summary>
        /// <param name="eventHolder">The request envelope to inspect.</param>
        /// <returns>The response payload, or <see langword="null"/> when no response is available.</returns>
        public static object GetResponse(this EventHolder eventHolder)
            => eventHolder.GetResponseHolder()?.BoxedPayload;
    }
}
