using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines;

internal class FinalizedNotification : StateMachineObserver
{
    public override async Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next)
    {
        await next();
        var stateflowsContext = ((IRootContext)context).Context.Context;
        if (stateflowsContext.ContextParentId != null)
        {
            context.Behavior.Send(new SubmachineFinalized());
        }
    }
}

/// <summary>Notifies an enclosing behavior after the embedded machine successfully finalizes.</summary>
/// <remarks>Queues a <see cref="SubmachineFinalized"/> notification when a parent context exists.
/// See <see href="https://github.com/Stateflows/framework/wiki/behavioral-groups">behavioral groups</see>.</remarks>
public static class FinalizedNotificationPolicy
{
    /// <summary>Registers finalization notification on a fluent machine definition.</summary>
    /// <param name="builder">The machine builder receiving the observer.</param>
    /// <returns>The original machine builder.</returns>
    public static IStateMachineBuilder AddFinalizedNotificationPolicy(this IStateMachineBuilder builder)
        => builder.AddObserver((_, _) => new FinalizedNotification());
    
    /// <summary>Registers finalization notification on an already defined machine.</summary>
    /// <param name="builder">The utilities builder receiving the observer.</param>
    /// <returns>The original utilities builder.</returns>
    public static IStateMachineUtilsBuilder AddFinalizedNotificationPolicy(this IStateMachineUtilsBuilder builder)
        => builder.AddObserver((_, _) => new FinalizedNotification());
}
