using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Activities;

internal class FinalizedNotification : ActivityObserver
{
    public override async Task OnActivityFinalizeAsync(IActivityFinalizationContext context, Func<Task> next)
    {
        await next();
        if (context.TryGetParentBehaviorContext(out var parentBehaviorContext))
        {
            parentBehaviorContext.Send(new DoActivityFinalized());
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
