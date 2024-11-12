using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Numerics;

namespace Stateflows.Common
{
    public interface ICompound
    {
        ICompound Add<TEvent>(TEvent @event);
        ICompound AddRange<TEvent>(params TEvent[] events);
    }

    public class CompoundRequest : IRequest<CompoundResponse>, ICompound
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<EventHolder> Events { get; set; } = new List<EventHolder>();

        ICompound ICompound.Add<TEvent>(TEvent @event)
            => Add(@event);

        public CompoundRequest Add<TEvent>(TEvent @event)
        {
            Events.Add(new EventHolder<TEvent>() { Payload = @event });

            return this;
        }

        ICompound ICompound.AddRange<TEvent>(params TEvent[] events)
            => AddRange(events);

        public CompoundRequest AddRange<TEvent>(params TEvent[] events)
        {
            Events.AddRange(events.Select(@event => new EventHolder<TEvent>() { Payload = @event }));

            return this;
        }
    }
}
