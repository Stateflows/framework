using Stateflows.Activities.Context.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Activities;

internal class FinalizedNotification : ActivityObserver
{
    public override void AfterActivityFinalize(IActivityFinalizationContext context)
    {
        var stateflowsContext = ((IRootContext)context).Context.Context;
        if (stateflowsContext.ContextParentId != null)
        {
            context.Behavior.Send(new DoActivityFinalized());
        }
    }
}

public static class FinalizedNotificationPolicy
{
    public static IActivityBuilder AddFinalizedNotificationPolicy(this IActivityBuilder builder)
        => builder.AddObserver(_ => new FinalizedNotification());
    
    public static IActivityUtilsBuilder AddFinalizedNotificationPolicy(this IActivityUtilsBuilder builder)
        => builder.AddObserver(_ => new FinalizedNotification());
}
