using System;

namespace Stateflows.Common
{
    public static class EventInfoExtensions
    {
        public static string GetEventName(this Type @type)
            => @type.IsSubclassOf(typeof(Event))
                ? EventInfo.GetName(@type)
                : @type.GetTokenName();
    }
}
