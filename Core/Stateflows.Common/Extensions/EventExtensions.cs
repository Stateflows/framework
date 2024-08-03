using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common.Extensions
{
    public static class EventExtensions
    {
        public static EventValidation Validate(this Event @event)
        {
            var validationResults = new List<ValidationResult>();
            bool isValid = true;

            if (@event is CompoundRequest compoundRequest)
            {
                var results = new List<RequestResult>();
                foreach (var ev in compoundRequest.Events)
                {
                    var validation = ev.Validate();
                    var status = validation.IsValid
                        ? EventStatus.Omitted
                        : EventStatus.Invalid;

                    if (!validation.IsValid)
                    {
                        isValid = false;
                    }

                    results.Add(new RequestResult(ev, @event.GetResponse(), status, validation));
                }

                if (!isValid)
                {
                    compoundRequest.Respond(new CompoundResponse() { Results = results });
                }
            }
            else
            {
                var validationContext = new ValidationContext(@event, serviceProvider: null, items: null);

                isValid = Validator.TryValidateObject(@event, validationContext, validationResults, true);
            }

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

        //public static bool IsPayloadEvent(this Event @event)
        //    => @event.GetType().IsSubclassOfRawGeneric(typeof(Event<>));

        //public static TPayload GetPayload<TPayload>(this Event @event)
        //{
        //    if (!@event.IsPayloadEvent())
        //    {
        //        return default;
        //    }

        //    return (TPayload)@event.GetType().GetProperty("Payload").GetValue(@event);
        //}

        public static void Respond(this Event @event, Response response)
        {
            if (@event.IsRequest())
            {
                @event.GetType().GetMethod("Respond").Invoke(@event, new object[] { response });
            }
        }
    }
}
