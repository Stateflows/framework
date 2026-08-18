using Stateflows.Common.Context.Classes;
using Stateflows.Actions;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Action;

internal class FinalizedNotification : ActionObserver
{
    public override void AfterActionFinalize(IActionDelegateContext context)
    {
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

public static class FinalizedNotificationPolicy
{
    public static IActionBuilder AddFinalizedNotificationPolicy(this IActionBuilder builder)
        => builder.AddObserver(_ => new FinalizedNotification());
    
    public static IActionBuilder<TAction> AddFinalizedNotificationPolicy<TAction>(this IActionBuilder<TAction> builder)
        where TAction : class, IAction
        => builder.AddObserver(_ => new FinalizedNotification());
}
