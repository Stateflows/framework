using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Handles an activity-runtime event using its declared event type.</summary>
    public interface IActivityEventHandler
    {
        /// <summary>The event type handled by this service.</summary>
        Type EventType { get; }

        /// <summary>Attempts to handle an event of the specified type.</summary>
        /// <typeparam name="TEvent">The event type being processed.</typeparam>
        /// <param name="context">The activity event context.</param>
        /// <returns>The resulting event status.</returns>
        Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context);
    }
}
