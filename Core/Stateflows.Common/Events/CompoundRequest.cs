﻿using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public interface ICompoundRequestBuilder
    {
        ICompoundRequestBuilder Add<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null);
        ICompoundRequestBuilder AddRange<TEvent>(params TEvent[] events);
    }

    public class CompoundRequestBuilderRequest : IRequest<CompoundResponse>, ICompoundRequestBuilder
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<EventHolder> Events { get; set; } = new List<EventHolder>();

        ICompoundRequestBuilder ICompoundRequestBuilder.Add<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
            => Add(@event, headers);

        public CompoundRequestBuilderRequest Add<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            Events.Add(@event.ToTypedEventHolder(headers));

            return this;
        }

        ICompoundRequestBuilder ICompoundRequestBuilder.AddRange<TEvent>(params TEvent[] events)
            => AddRange(events);

        public CompoundRequestBuilderRequest AddRange<TEvent>(params TEvent[] events)
        {
            Events.AddRange(events.Select(@event => @event.ToTypedEventHolder()));

            return this;
        }
    }
}
