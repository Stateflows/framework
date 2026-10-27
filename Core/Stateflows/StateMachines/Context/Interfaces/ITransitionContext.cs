namespace Stateflows.StateMachines.Context.Interfaces
{
    /// <summary>Combines typed event metadata with transition source and target contexts.</summary>
    /// <typeparam name="TEvent">The trigger payload type; structural-event overloads supply <c>StructuralEvent</c>.</typeparam>
    public interface ITransitionContext<out TEvent> : IEventContext<TEvent>, ITransitionContext
    { }
}
