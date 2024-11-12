using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public static class Event
    {
        public static string GetName(Type @type)
            => @type.GetReadableName();

        //public Guid Id { get; set; } = Guid.NewGuid();

        //[JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        //public List<EventHeader> Headers { get; set; } = new List<EventHeader>();

        //public DateTime SentAt { get; set; }

        //public virtual string Name => GetType().GetEventName();
    }

    public static class Event<TEvent>
    {
        public static string Name => Event.GetName(typeof(TEvent));
    }
}
