using Stateflows.Common.Extensions;
using System;
using System.Linq;

namespace Stateflows.Common
{
    public static class EventInfoExtensions
    {
        public static string GetEventName(this Type @type)
            => @type.GetReadableName();
    }
}
