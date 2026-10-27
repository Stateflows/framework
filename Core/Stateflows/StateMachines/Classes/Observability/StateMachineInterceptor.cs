using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Base event interceptor that forwards the event and returns the continuation's status unchanged.</summary>
    /// <remarks>Override to implement middleware or short-circuit processing.
    /// See <see href="https://github.com/Stateflows/framework/wiki/observability">observability</see>.</remarks>
    public abstract class StateMachineInterceptor : IStateMachineInterceptor
    {
        /// <inheritdoc/>
        public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
            => next();
    }
}