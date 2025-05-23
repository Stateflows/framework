using Stateflows.StateMachines;
using WarszawskieDniInformatyki.StateMachines.Document.Events;

namespace WarszawskieDniInformatyki.StateMachines.Document.Guards;

public class PayGuard : ITransitionGuard<Pay>
{
    public Task<bool> GuardAsync(Pay @event)
    {
        return Task.FromResult(@event.Amount < 1000);
    }
}