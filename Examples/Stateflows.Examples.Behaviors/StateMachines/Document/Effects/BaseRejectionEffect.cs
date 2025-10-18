using Stateflows.StateMachines;
using Stateflows.Examples.Common.Events;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Effects;

public abstract class BaseRejectionEffect(
    IStateMachineContext stateMachineContext,
    string reason
) : ITransitionEffect
{
    public Task EffectAsync()
    {
        stateMachineContext.Publish(new RejectionNotification { Reason = reason });
        
        return Task.CompletedTask;
    }
}