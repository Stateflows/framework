using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineInterceptor
    {
        Task AfterHydrateAsync(IStateMachineActionContext context);

        Task BeforeDehydrateAsync(IStateMachineActionContext context);

        [Obsolete]
        Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context);

        [Obsolete]
        Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, EventStatus eventStatus);

        Task<EventStatus> ProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, Func<IEventActionContext<TEvent>, Task<EventStatus>> next);
    }
}
