namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface ITransitionContext<out TEvent> : IEventContext<TEvent>, ITransitionContext
    { }
}
