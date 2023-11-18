using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common.Extensions
{
    public static class EventExtensions
    {
        public static EventValidation Validate(this Event @event)
        {
            var validationContext = new ValidationContext(@event, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(@event, validationContext, validationResults, true);

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
