using System.Threading.Tasks;

namespace Stateflows.Activities
{
    /// <summary>Defines a node action that asynchronously produces an event.</summary>
    /// <typeparam name="TEvent">The event type produced by the node.</typeparam>
    public interface IEventActionNode<TEvent> : IActivityNode
    {
        /// <summary>Creates the event to be sent or published by this node.</summary>
        /// <returns>The generated event.</returns>
        Task<TEvent> GenerateEventAsync();
    }
}
