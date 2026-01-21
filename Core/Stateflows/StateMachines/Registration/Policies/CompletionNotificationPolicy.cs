using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines;

internal class CompletionNotification : StateMachineInterceptor
{
    public override void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
    {
        var stateflowsContext = ((IRootContext)context).Context.Context;
        if (stateflowsContext.ContextParentId != null)
        {
            context.Behavior.Send(new Completion());
        }
    }
}

public static class CompletionNotificationPolicy
{
    public static IStateMachineBuilder AddCompletionNotificationPolicy(this IStateMachineBuilder builder)
        => builder.AddInterceptor((_, _) => new CompletionNotification());
}
