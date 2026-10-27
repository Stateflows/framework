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

/// <summary>Notifies an enclosing state machine after the embedded machine consumes an event or initializes.</summary>
/// <remarks>The policy queues a completion event for a parent whose behavior kind is a state machine.
/// See <see href="https://github.com/Stateflows/framework/wiki/behavioral-groups">behavioral groups</see>.</remarks>
public static class CompletionNotificationPolicy
{
    /// <summary>Adds parent completion notification to a fluent machine definition.</summary>
    /// <param name="builder">The machine builder receiving the interceptor.</param>
    /// <returns>The original machine builder.</returns>
    public static IStateMachineBuilder AddCompletionNotificationPolicy(this IStateMachineBuilder builder)
        => builder.AddInterceptor((_, _) => new CompletionNotification());
    
    /// <summary>Adds parent completion notification to an already defined machine.</summary>
    /// <param name="builder">The machine utilities builder receiving the interceptor.</param>
    /// <returns>The original utilities builder.</returns>
    public static IStateMachineUtilsBuilder AddCompletionNotificationPolicy(this IStateMachineUtilsBuilder builder)
        => builder.AddInterceptor((_, _) => new CompletionNotification());
}
