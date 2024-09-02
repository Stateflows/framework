using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<EventHeader> Headers { get; set; } = new List<EventHeader>();

        public DateTime SentAt { get; set; }

        public virtual string Name => GetType().GetEventName();
    }

    public class Event<TPayload> : Event
    {
        public Event()
        {
            Payload = default;
        }

        public TPayload Payload { get; set; }
    }

    //public static class EventInfo<TEvent>
    //{
    //    public static string Name => EventInfo.GetName(typeof(TEvent));
    //}

    //public static class EventInfo
    //{
    //    public static string GetName(Type @type)
    //        => @type.GetReadableName();
    //}
}
