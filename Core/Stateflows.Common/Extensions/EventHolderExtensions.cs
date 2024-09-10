using Stateflows.Common.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common
{
    public static class EventHolderExtensions
    {
        public static bool IsRequest(this EventHolder eventHolder)
            => eventHolder.PayloadType.IsImplementerOfRawGeneric(typeof(IRequest<>));

        public static bool IsRespondedTo(this EventHolder eventHolder)
            => eventHolder.IsRequest() && ResponseHolder.IsResponded(eventHolder.BoxedPayload);

        public static EventHolder GetResponseHolder(this EventHolder eventHolder)
            => eventHolder.IsRequest()
            ? ResponseHolder.GetResponseOrDefault(eventHolder.BoxedPayload)
            : null;

        public static object GetResponse(this EventHolder eventHolder)
            => eventHolder.GetResponseHolder()?.BoxedPayload ?? default;

        public static EventValidation Validate(this EventHolder eventHolder)
        {
            var validationResults = new List<ValidationResult>();
            bool isValid = true;

            if (eventHolder is EventHolder<CompoundRequest> compoundRequestHolder)
            {
                var compoundRequest = compoundRequestHolder.Payload;
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

                    results.Add(new RequestResult(
                        ev,
                        null,
                        status,
                        validation
                    ));
                }

                if (!isValid)
                {
                    compoundRequest.Respond(new CompoundResponse() { Results = results });
                }
            }
            else
            {
                if (eventHolder.PayloadType.IsClass)
                {
                    var validationContext = new ValidationContext(eventHolder.BoxedPayload, serviceProvider: null, items: null);

                    isValid = Validator.TryValidateObject(eventHolder.BoxedPayload, validationContext, validationResults, true);
                }
                else
                {
                    isValid = true;
                }
            }

            return new EventValidation(isValid, validationResults);
        }
    }
}
