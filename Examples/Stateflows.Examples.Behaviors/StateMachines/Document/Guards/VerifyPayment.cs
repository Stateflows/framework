using Stateflows.Examples.Common.Events;
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Guards;

public class VerifyPayment : ITransitionGuard<PaymentBooked>
{
    public Task<bool> GuardAsync(PaymentBooked @event)
    {
        return Task.FromResult(@event.Amount < 1000);
    }
}