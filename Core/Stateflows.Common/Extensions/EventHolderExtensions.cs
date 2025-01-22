using Stateflows.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    public static class EventHolderExtensions
    {
        public static void Respond(this EventHolder eventHolder, EventHolder response)
        {
            if (!eventHolder.IsRequest())
            {
                throw new InvalidOperationException("Event type does not implement IRequest<> interface and cannot be responded to.");
            }

            ResponseHolder.Respond(eventHolder.BoxedPayload, response);
        }

        public static bool IsRequest(this EventHolder eventHolder)
            => eventHolder.PayloadType.IsImplementerOfRawGeneric(typeof(IRequest<>));

        public static bool IsRespondedTo(this EventHolder eventHolder)
            => eventHolder.IsRequest() && ResponseHolder.IsResponded(eventHolder.BoxedPayload);

        public static EventHolder GetResponseHolder(this EventHolder eventHolder)
            => eventHolder.IsRequest()
                ? ResponseHolder.GetResponseOrDefault(eventHolder.BoxedPayload)
                : null;

        public static object GetResponse(this EventHolder eventHolder)
            => eventHolder.GetResponseHolder()?.BoxedPayload;
    }
}
