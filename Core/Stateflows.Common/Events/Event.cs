using System;
using System.Collections.Generic;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public class Event
    {
        public Event()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public virtual string EventName => GetType().FullName;

        public List<EventHeader> Headers { get; set; } = new List<EventHeader>();
    }

    public static class EventInfo<TEvent>
        where TEvent : Event, new()
    {
        public static string Name => EventInfo.GetName(typeof(TEvent));
    }

    public static class EventInfo
    {
        public static string GetName(Type @type)
            => (@type.GetUninitializedInstance() as Event).EventName;
    }
}
