using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Activities;

internal class CompletionNotification : ActivityInterceptor
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
            if (
                status is EventStatus.Consumed or EventStatus.Initialized &&
                context.TryGetParentBehaviorContext(out var parentBehaviorContext) &&
                parentBehaviorContext.Id.Type == BehaviorType.StateMachine
            )
            {
                parentBehaviorContext.Send(new Completion());
            // }
            // var stateflowsContext = ((IRootContext)context).Context.Context;
            // if (
            //     stateflowsContext.ContextParentId != null &&
            //     stateflowsContext.ContextParentId.Value.Type == BehaviorType.StateMachine &&
            //     status is EventStatus.Consumed or EventStatus.Initialized
            // )
            // {
            }
        }

    }
}

/// <summary>Provides the activity completion-notification registration policy.</summary>
public static class CompletionNotificationPolicy
{
    /// <summary>Notifies a parent state machine when this activity completes event processing.</summary>
    /// <param name="builder">The activity graph builder.</param>
    /// <returns>The activity builder for further configuration.</returns>
    /// <remarks>Completion is sent to a parent state machine after event processing returns <see cref="EventStatus.Consumed"/> or <see cref="EventStatus.Initialized"/>.</remarks>
    public static IActivityBuilder AddCompletionNotificationPolicy(this IActivityBuilder builder)
        => builder.AddInterceptor(_ => new CompletionNotification());
    
    /// <summary>Enables parent state-machine completion notifications through activity-wide utilities.</summary>
    /// <param name="builder">The activity utility builder.</param>
    /// <returns>The activity utility builder for further configuration.</returns>
    public static IActivityUtilsBuilder AddCompletionNotificationPolicy(this IActivityUtilsBuilder builder)
        => builder.AddInterceptor(_ => new CompletionNotification());
}
