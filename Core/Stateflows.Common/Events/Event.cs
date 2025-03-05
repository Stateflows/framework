using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public static class Event
    {
        public static string GetName(Type @type)
            => @type.GetEventName();
        
        public static string GetShortName(Type @type)
            => @type.GetShortEventName();
    }

    public static class Event<TEvent>
    {
        public static string Name => Event.GetName(typeof(TEvent));
        
        public static string ShortName => Event.GetShortName(typeof(TEvent));
    }
}
