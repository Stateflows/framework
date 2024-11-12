using Stateflows.Common.Extensions;
using System;

namespace Stateflows.Common
{
    public static class EventInfoExtensions
    {
        public static string GetEventName(this Type @type)
        {
            return @type.GetReadableName();
        }
    }
}
