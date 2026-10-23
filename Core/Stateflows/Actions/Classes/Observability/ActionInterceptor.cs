using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Actions;

public abstract class ActionInterceptor : IActionInterceptor
{
    public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        => next();
}