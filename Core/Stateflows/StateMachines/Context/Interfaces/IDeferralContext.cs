namespace Stateflows.StateMachines.Context.Interfaces
{
    /// <summary>Combines the candidate event with the state evaluating its deferral rule.</summary>
    /// <typeparam name="TEvent">The event type whose processing may be deferred.</typeparam>
    public interface IDeferralContext<out TEvent> : IEventContext<TEvent>, IDeferralContext;
}
