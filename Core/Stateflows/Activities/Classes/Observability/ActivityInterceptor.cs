using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public abstract class ActivityInterceptor : IActivityInterceptor
    {
        public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
            => next();
    }
}
