using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Stateflows.Common.Utilities;

namespace Stateflows.Common
{
    public interface ICompoundRequestBuilder
    {
        ICompoundRequestBuilder Add<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null);
        ICompoundRequestBuilder AddRange<TEvent>(params TEvent[] events);
    }

    public class CompoundRequest : IRequest<CompoundResponse>, ICompoundRequestBuilder
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<EventHolder> Events { get; set; } = [];

        ICompoundRequestBuilder ICompoundRequestBuilder.Add<TEvent>(TEvent @event, IEnumerable<EventHeader> headers)
            => Add(@event, headers);

        public CompoundRequest Add<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            Events.Add(@event.ToTypedEventHolder(headers));

            return this;
        }

        ICompoundRequestBuilder ICompoundRequestBuilder.AddRange<TEvent>(params TEvent[] events)
            => AddRange(events);

        public CompoundRequest AddRange<TEvent>(params TEvent[] events)
        {
            Events.AddRange(events.Select(@event => @event.ToTypedEventHolder()));

            return this;
        }
    }
}
