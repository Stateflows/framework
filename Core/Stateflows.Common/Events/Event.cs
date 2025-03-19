using System;

namespace Stateflows.Common
{
    public static class Event
    {
        public static string GetName(Type @type)
            => @type.GetEventName();
    }

    public static class Event<TEvent>
    {
        public static string Name => Event.GetName(typeof(TEvent));
    }
}
