namespace Stateflows.Common.Extensions
{
    public static class EventExtensions
    {
        public static bool IsRequest(this Event @event)
        {
            var type = @event.GetType().BaseType;
            while (type != null)
            {
                if (type.FullName.StartsWith("Stateflows.Common.Request`1"))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
        public static bool IsInitializationRequest(this Event @event)
        {
            var type = @event.GetType().BaseType;
            while (type != null)
            {
                if (type.FullName.StartsWith("Stateflows.Common.InitializationRequest`1"))
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
