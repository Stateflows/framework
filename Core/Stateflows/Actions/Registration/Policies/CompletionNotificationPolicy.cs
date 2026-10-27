using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Context.Classes;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Actions;

internal class CompletionNotification : ActionInterceptor
{
    public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
    {
        var status = EventStatus.Failed;
        try
        {
            return status = await next();
        }
        finally
        {
            if (status == EventStatus.Consumed)
            {
                var stateflowsContext = ((BaseContext)context).Context;
                if (
                    stateflowsContext.ContextParentId != null &&
                    stateflowsContext.ContextParentId.Value.Type == BehaviorType.StateMachine &&
                    status is EventStatus.Consumed or EventStatus.Initialized &&
                    context.TryGetParentBehaviorContext(out var parentBehaviorContext)
                )
                {
                    parentBehaviorContext.Send(new Completion());
                }
            }
        }

    }
}

/// <summary>Provides the action completion-notification registration policy.</summary>
public static class CompletionNotificationPolicy
{
    /// <summary>Notifies a parent state machine when a nested action consumes an event.</summary>
    /// <param name="builder">The delegate-based action builder.</param>
    /// <returns>The action builder for further configuration.</returns>
    /// <remarks>The notification is sent only when the action is nested under a state machine and event processing returns <see cref="EventStatus.Consumed"/>.</remarks>
    public static IActionBuilder AddCompletionNotificationPolicy(this IActionBuilder builder)
        => builder.AddInterceptor(_ => new CompletionNotification());
    
    /// <summary>Notifies a parent state machine when a nested typed action consumes an event.</summary>
    /// <typeparam name="TAction">The typed action implementation.</typeparam>
    /// <param name="builder">The typed action builder.</param>
    /// <returns>The action builder for further configuration.</returns>
    public static IActionBuilder<TAction> AddCompletionNotificationPolicy<TAction>(this IActionBuilder<TAction> builder)
        where TAction : class, IAction
        => builder.AddInterceptor(_ => new CompletionNotification());
}
