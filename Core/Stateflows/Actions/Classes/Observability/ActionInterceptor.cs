using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Actions;

/// <summary>Base interceptor that forwards each event to the next processor by default.</summary>
/// <remarks>Override <see cref="IActionInterceptor.ProcessEventAsync{TEvent}"/> only for behavior that requires interception.</remarks>
public abstract class ActionInterceptor : IActionInterceptor
{
    /// <inheritdoc/>
    public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        => next();
}