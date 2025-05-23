using Stateflows.Examples.Common.Events;
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Guards;

public class PayGuard : ITransitionGuard<Pay>
{
    public Task<bool> GuardAsync(Pay @event)
    {
        return Task.FromResult(@event.Amount < 1000);
    }
}