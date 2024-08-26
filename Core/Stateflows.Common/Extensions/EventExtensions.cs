using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common.Extensions
{
    public static class EventExtensions
    {
        public static EventValidation Validate<TEvent>(this TEvent @event)
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

        public static EventHolder GetResponse<TEvent>(this TEvent @event)
            => @event is IRequest request
            ? request.GeneralResponseHolder
            : null;

        public static void Respond(this Event @event, Response response)
        {
            if (@event.IsRequest())
            {
                @event.GetType().GetMethod("Respond").Invoke(@event, new object[] { response });
            }
        }
    }
}
