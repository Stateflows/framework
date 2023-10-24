namespace Stateflows.Common.Extensions
{
    public static class EventExtensions
    {
        //public static bool Validate(this Event @event)
        //{
        //    var validationContext = new ValidationContext(@event, serviceProvider: null, items: null);
        //    var validationResults = new List<ValidationResult>();

        //    bool isValid = Validator.TryValidateObject(@event, validationContext, validationResults, true);

        //    //@event.Validation = new EventValidation(isValid, validationResults);

        //    return isValid;
        //}

        //public static void SetValidation(this Event @event, EventValidation validation)
        //{
        //    //@event.Validation = validation;
        //}

        public static bool IsRequest(this Event @event)
            => @event.GetType().IsSubclassOfRawGeneric(typeof(Request<>));

        public static bool IsInitializationRequest(this Event @event)
        {
            var type = @event.GetType().BaseType;
            while (type != null)
            {
                if (type.IsSubclassOf(typeof(InitializationRequest)))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        public static Response GetResponse(this Event @event)
        {
            if (!@event.IsRequest())
            {
                return null;
            }

            return @event.GetType().GetProperty("Response").GetValue(@event) as Response;
        }

        public static void Respond(this Event @event, Response response)
        {
            if (@event.IsRequest())
            {
                @event.GetType().GetMethod("Respond").Invoke(@event, new object[] { response });
            }
        }
    }
}
