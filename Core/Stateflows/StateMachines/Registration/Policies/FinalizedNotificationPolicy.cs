using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines;

internal class FinalizedNotification : StateMachineObserver
{
    public override void AfterStateMachineFinalize(IStateMachineActionContext context)
    {
        var stateflowsContext = ((IRootContext)context).Context.Context;
        if (stateflowsContext.ContextParentId != null)
        {
            context.Behavior.Send(new SubmachineFinalized());
        }
    }
}

public static class FinalizedNotificationPolicy
{
    public static IStateMachineBuilder AddFinalizedNotificationPolicy(this IStateMachineBuilder builder)
        => builder.AddObserver((_, _) => new FinalizedNotification());
    
    public static IStateMachineUtilsBuilder AddFinalizedNotificationPolicy(this IStateMachineUtilsBuilder builder)
        => builder.AddObserver((_, _) => new FinalizedNotification());
}
