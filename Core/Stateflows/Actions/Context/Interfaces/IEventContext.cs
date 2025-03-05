namespace Stateflows.Actions.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IActionDelegateContext
    {
        TEvent Event { get; }
    }
}
