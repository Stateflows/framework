using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Activities;

internal class CompletionNotification : ActivityInterceptor
{
    public override void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
    {
        if (eventStatus == EventStatus.Consumed)
        {
            var stateflowsContext = ((IRootContext)context).Context.Context;
            if (stateflowsContext.ContextParentId != null)
            {
                context.Behavior.Send(new Completion());
            }
        }
    }
}

public static class CompletionNotificationPolicy
{
    public static IActivityBuilder AddCompletionNotificationPolicy(this IActivityBuilder builder)
        => builder.AddInterceptor(_ => new CompletionNotification());
    
    public static IActivityUtilsBuilder AddCompletionNotificationPolicy(this IActivityUtilsBuilder builder)
        => builder.AddInterceptor(_ => new CompletionNotification());
}
