using System;

namespace Stateflows.Common.Extensions
{
    public static class EventInfoExtensions
    {
        public static string GetEventName(this Type @type)
        {
            if (!@type.IsSubclassOf(typeof(Event)))
            {
                throw new ArgumentException("Given type is not subclass of Event class");
            }

            var @event = Activator.CreateInstance(@type) as Event;
            return @event.Name;
        }
    }
}
