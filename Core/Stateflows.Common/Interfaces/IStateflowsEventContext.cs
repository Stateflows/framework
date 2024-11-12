namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsEventContext<out TEvent>
    {
        TEvent Event { get; }
    }
}
