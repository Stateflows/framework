namespace Stateflows.StateMachines
{
    public interface ITransitionContext<out TEvent> : IEventContext<TEvent>, ITransitionContext
    { }
}
