using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines;

internal class CompletionNotification : StateMachineInterceptor
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
            var stateflowsContext = ((IRootContext)context).Context.Context;
            if (
                stateflowsContext.ContextParentId != null &&
                stateflowsContext.ContextParentId.Value.Type == BehaviorType.StateMachine &&
                status is EventStatus.Consumed or EventStatus.Initialized
            )
            {
                context.Behavior.Send(new Completion());
            }
        }

    }
}

public static class CompletionNotificationPolicy
{
    public static IStateMachineBuilder AddCompletionNotificationPolicy(this IStateMachineBuilder builder)
        => builder.AddInterceptor((_, _) => new CompletionNotification());
    
    public static IStateMachineUtilsBuilder AddCompletionNotificationPolicy(this IStateMachineUtilsBuilder builder)
        => builder.AddInterceptor((_, _) => new CompletionNotification());
}
