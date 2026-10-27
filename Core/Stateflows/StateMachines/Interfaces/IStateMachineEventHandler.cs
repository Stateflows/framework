using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Handles framework or custom events alongside normal state-machine transition processing.</summary>
    public interface IStateMachineEventHandler
    {
        /// <summary>The event payload type handled by this service.</summary>
        Type EventType { get; }

        /// <summary>Attempts to handle the supplied event in the machine execution context.</summary>
        /// <typeparam name="TEvent">The event payload type.</typeparam>
        /// <param name="context">The event envelope and machine execution context.</param>
        /// <returns>The handling status indicating whether processing consumed, rejected, or otherwise handled the event.</returns>
        Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
;
    }
}
