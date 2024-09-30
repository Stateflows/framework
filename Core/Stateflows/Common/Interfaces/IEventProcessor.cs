using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    internal interface IEventProcessor
    {
        string BehaviorType { get; }

        Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event, List<Exception> exceptions)
            where TEvent : Event, new();
    }
}
