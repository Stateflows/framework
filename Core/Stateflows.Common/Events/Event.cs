using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public static class Event
    {
        public static string GetName(Type @type)
            => @type.GetReadableName();
    }

    public static class Event<TEvent>
    {
        public static string Name => Event.GetName(typeof(TEvent));
    }
}
