using System.Threading.Tasks;

namespace Stateflows.Activities
{
    /// <summary>Defines an event-producing node that sends its event to a selected behavior instance.</summary>
    /// <typeparam name="TEvent">The event type sent by the node.</typeparam>
    public interface ISendEventActionNode<TEvent> : IEventActionNode<TEvent>
    {
        /// <summary>Selects the behavior instance that receives the generated event.</summary>
        /// <returns>The identifier of the target behavior instance.</returns>
        Task<BehaviorId> SelectTargetAsync();
    }
}
