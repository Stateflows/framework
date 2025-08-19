using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public static class EventInfoExtensions
    {
        public static string GetEventName(this Type @type)
            => @type.GetReadableName(TypedElements.Events);
    }
}
