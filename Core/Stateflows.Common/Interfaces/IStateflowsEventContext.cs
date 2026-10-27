namespace Stateflows.Common.Interfaces
{
    /// <summary>Provides the event payload associated with a Stateflows event operation.</summary>
    /// <typeparam name="TEvent">The event payload type.</typeparam>
    public interface IStateflowsEventContext<out TEvent>
    {
        /// <summary>Gets the event payload.</summary>
        TEvent Event { get; }
    }
}
