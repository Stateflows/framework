using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Wraps event dispatch initiated by a Stateflows client.</summary>
    public interface IStateflowsClientInterceptor
    {
        /// <summary>Wraps dispatch of an event to its target behavior.</summary>
        /// <typeparam name="TEvent">The event payload type.</typeparam>
        /// <param name="targetBehaviorId">The intended recipient behavior.</param>
        /// <param name="event">The event payload.</param>
        /// <param name="headers">The event's mutable delivery headers.</param>
        /// <param name="next">Starts the remaining interceptors and dispatch, then completes without waiting for processing to finish; await it at most once to continue.</param>
        Task DispatchEventAsync<TEvent>(BehaviorId targetBehaviorId, TEvent @event,
            IDictionary<string, EventHeader> headers, Func<Task> next);
    }
}
