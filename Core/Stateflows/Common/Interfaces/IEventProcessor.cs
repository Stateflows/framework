using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Processes events for the behavior types handled by an engine processor.</summary>
    public interface IEventProcessor
    {
        /// <summary>Gets the behavior type names supported by this processor.</summary>
        string[] BehaviorTypes { get; }
        
        /// <summary>Processes an event envelope for a behavior instance.</summary>
        /// <typeparam name="TEvent">The payload type carried by the envelope.</typeparam>
        /// <param name="id">The target behavior identifier.</param>
        /// <param name="eventHolder">The event envelope to process.</param>
        /// <param name="exceptions">A collection that receives processing exceptions.</param>
        /// <returns>The status produced by event processing.</returns>
        Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions);

        /// <summary>Cancels active processing for a behavior instance.</summary>
        /// <param name="id">The behavior whose processing is cancelled.</param>
        Task CancelProcessingAsync(BehaviorId id);
    }
}
