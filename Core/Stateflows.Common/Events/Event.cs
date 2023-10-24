using System;
using System.Collections.Generic;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public abstract class Event
    {
        public Guid Id { get; } = Guid.NewGuid();

        public virtual string Name => GetType().FullName;

        public List<EventHeader> Headers { get; } = new List<EventHeader>();
    }

    public static class EventInfo<TEvent>
        where TEvent : Event
    {
        public static string Name => EventInfo.GetName(typeof(TEvent));
    }

    public static class EventInfo
    {
        public static string GetName(Type @type)
            => (@type.GetUninitializedInstance() as Event).Name;
    }
}
