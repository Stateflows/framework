using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities
{
    /// <summary>Base interceptor that forwards event processing to the next processor by default.</summary>
    /// <remarks>Override <see cref="IActivityInterceptor.ProcessEventAsync{TEvent}"/> only for behavior that requires interception.</remarks>
    public abstract class ActivityInterceptor : IActivityInterceptor
    {
        /// <inheritdoc/>
        public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
            => next();
    }
}
