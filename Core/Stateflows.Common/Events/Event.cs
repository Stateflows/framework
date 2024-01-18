﻿using System;
using System.Collections.Generic;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public class Event : Token
    {
        public List<EventHeader> Headers { get; set; } = new List<EventHeader>();
    }

    public class Event<TPayload> : Event
    {
        public Event()
        {
            Payload = default;
        }

        public TPayload Payload { get; set; }
    }

    public static class EventInfo<TEvent>
        where TEvent : Event, new()
    {
        public static string Name => EventInfo.GetName(typeof(TEvent));
    }

    public static class EventInfo
    {
        public static string GetName(Type @type)
            => (@type.GetUninitializedInstance() as Event).Name;
    }
}
