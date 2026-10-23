using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachineInterceptor : IStateMachineInterceptor
    {
        public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
            => next();
    }
}