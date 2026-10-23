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

public static class CompletionNotificationPolicy
{
    public static IActionBuilder AddCompletionNotificationPolicy(this IActionBuilder builder)
        => builder.AddInterceptor(_ => new CompletionNotification());
    
    public static IActionBuilder<TAction> AddCompletionNotificationPolicy<TAction>(this IActionBuilder<TAction> builder)
        where TAction : class, IAction
        => builder.AddInterceptor(_ => new CompletionNotification());
}
