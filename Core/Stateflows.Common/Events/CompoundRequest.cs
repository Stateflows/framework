using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public class CompoundRequest : Request<CompoundResponse>
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<EventHolder> Events { get; set; } = new List<EventHolder>();

        public void AddEvent<TEvent>(TEvent @event)
            => Events.Add(new EventHolder<TEvent>() { Payload = @event });

        public void AddEvents<TEvent>(IEnumerable<TEvent> events)
            => Events.AddRange(events.Select(@event => new EventHolder<TEvent>() { Payload = @event }));
    }
}
