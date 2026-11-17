namespace Stateflows.StateMachines.Context.Interfaces
{
    /// <summary>Combines typed event metadata with transition source and target contexts.</summary>
    /// <typeparam name="TEvent">The trigger payload type; payload overloads supply <c>Payload</c>.</typeparam>
    public interface ITransitionContext<out TEvent> : IEventContext<TEvent>, ITransitionContext
    { }
}
