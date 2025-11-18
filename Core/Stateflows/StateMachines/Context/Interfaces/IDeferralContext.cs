namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IDeferralContext<out TEvent> : IEventContext<TEvent>, IDeferralContext
    { }
}
