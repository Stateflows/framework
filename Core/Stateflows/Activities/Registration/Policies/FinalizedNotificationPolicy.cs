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

/// <summary>Provides the activity finalized-notification registration policy.</summary>
public static class FinalizedNotificationPolicy
{
    /// <summary>Notifies the parent behavior after this activity has finalized.</summary>
    /// <param name="builder">The activity graph builder.</param>
    /// <returns>The activity builder for further configuration.</returns>
    /// <remarks>The notification is sent after the activity-finalization observer continuation completes.</remarks>
    public static IActivityBuilder AddFinalizedNotificationPolicy(this IActivityBuilder builder)
        => builder.AddObserver(_ => new FinalizedNotification());
    
    /// <summary>Enables parent notifications after finalization through activity-wide utilities.</summary>
    /// <param name="builder">The activity utility builder.</param>
    /// <returns>The activity utility builder for further configuration.</returns>
    public static IActivityUtilsBuilder AddFinalizedNotificationPolicy(this IActivityUtilsBuilder builder)
        => builder.AddObserver(_ => new FinalizedNotification());
}
