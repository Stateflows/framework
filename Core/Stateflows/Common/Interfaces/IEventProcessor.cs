using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    internal interface IEventProcessor
    {
        string BehaviorType { get; }
        
        Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions);

        Task CancelProcessingAsync(BehaviorId id);
    }
}
