using System;
using System.Threading.Tasks;
using Stateflows.Common.Context.Classes;
using Stateflows.Actions;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Action;

internal class FinalizedNotification : ActionObserver
{
    public override async Task OnActionFinalizeAsync(IActionDelegateContext context, Func<Task> next)
    {
        await next();
        var stateflowsContext = ((BaseContext)context).Context;
        if (
            stateflowsContext.ContextParentId != null &&
            context.TryGetParentBehaviorContext(out var parentBehaviorContext)
        )
        {
            parentBehaviorContext.Send(new DoActionFinalized());
        }
    }
}

/// <summary>Provides the action finalized-notification registration policy.</summary>
public static class FinalizedNotificationPolicy
{
    /// <summary>Notifies the parent behavior after an action has finalized.</summary>
    /// <param name="builder">The delegate-based action builder.</param>
    /// <returns>The action builder for further configuration.</returns>
    /// <remarks>The parent notification is sent only after the action-finalization observer continuation completes.</remarks>
    public static IActionBuilder AddFinalizedNotificationPolicy(this IActionBuilder builder)
        => builder.AddObserver(_ => new FinalizedNotification());
    
    /// <summary>Notifies the parent behavior after a typed action has finalized.</summary>
    /// <typeparam name="TAction">The typed action implementation.</typeparam>
    /// <param name="builder">The typed action builder.</param>
    /// <returns>The action builder for further configuration.</returns>
    public static IActionBuilder<TAction> AddFinalizedNotificationPolicy<TAction>(this IActionBuilder<TAction> builder)
        where TAction : class, IAction
        => builder.AddObserver(_ => new FinalizedNotification());
}
