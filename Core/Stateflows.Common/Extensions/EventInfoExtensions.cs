using Stateflows.Common.Extensions;
using System;

namespace Stateflows.Common
{
    public static class EventInfoExtensions
    {
        public static string GetEventName(this Type @type)
        {
            if (!@type.IsSubclassOf(typeof(Event)))
            {
                throw new ArgumentException("Given type is not subclass of Event class");
            }

            return @type.GetReadableName();
        }
    }
}
