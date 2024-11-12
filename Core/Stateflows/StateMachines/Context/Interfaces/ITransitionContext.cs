namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface ITransitionContext<out TEvent> : IEventActionContext<TEvent>, ITransitionContext
    { }
}
