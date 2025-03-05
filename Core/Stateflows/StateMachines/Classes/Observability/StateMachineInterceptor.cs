using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachineInterceptor : IStateMachineInterceptor
    {
        public virtual Task AfterHydrateAsync(IStateMachineActionContext context)
                => Task.CompletedTask;

        public virtual Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        [Obsolete]
        public virtual Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
            => Task.FromResult(true);

        [Obsolete]
        public virtual Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, EventStatus eventStatus)
            => Task.CompletedTask;

        public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, Func<IEventActionContext<TEvent>, Task<EventStatus>> next)
            => next(context);
    }
}