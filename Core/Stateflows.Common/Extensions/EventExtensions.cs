using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common.Extensions
{
    public static class EventExtensions
    {
        public static EventValidation Validate(this Event @event)
        {
            var objectToValidate = @event.GetPayload<object>() ?? @event;

            var validationContext = new ValidationContext(objectToValidate, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(objectToValidate, validationContext, validationResults, true);

            return new EventValidation(isValid, validationResults);
        }

        public static bool IsRequest(this Event @event)
            => @event.GetType().IsSubclassOfRawGeneric(typeof(Request<>));

        public static Type GetResponseType(this Event @event)
            => @event.GetType().GetGenericParameterOf(typeof(Request<>));

        public static TResponse GetResponse<TResponse>(this Event @event)
            where TResponse : Response, new()
        {
            if (!@event.IsRequest())
            {
                return null;
            }

            return @event.GetType().GetProperty("Response").GetValue(@event) as TResponse;
        }

        public static bool IsPayloadEvent(this Event @event)
            => @event.GetType().IsSubclassOfRawGeneric(typeof(Event<>));

        public static TPayload GetPayload<TPayload>(this Event @event)
        {
            if (!@event.IsPayloadEvent())
            {
                return default;
            }

            return (TPayload)@event.GetType().GetProperty("Payload").GetValue(@event);
        }

        public static Response GetResponse(this Event @event)
            => @event.GetResponse<Response>();

        public static void Respond(this Event @event, Response response)
        {
            if (@event.IsRequest())
            {
                @event.GetType().GetMethod("Respond").Invoke(@event, new object[] { response });
            }
        }
    }
}
