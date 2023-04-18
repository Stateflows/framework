using System;

namespace Stateflows.Common
{
    public class Event
    {
        public Guid Id { get; } = Guid.NewGuid();

        public virtual string Name => GetType().Name;
    }

    public sealed class EventInfo<TEvent>
        where TEvent : Event, new()
    {
        public static string Name => new TEvent().Name;
    }
}
