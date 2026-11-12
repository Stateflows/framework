using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Entities.Enums;

namespace Stateflows.Activities
{
    /// <summary>Defines an event-producing node that publishes its generated events.</summary>
    /// <typeparam name="TEvent">The event type published by the node.</typeparam>
    public interface IPublishEventActionNode<TEvent> : IActivityNode
    {
        /// <summary>Gets the behaviors on whose behalf events are published.</summary>
        static virtual PublishScope PublishScope => PublishScope.Self;
        /// <summary>Gets the notification lifetime in seconds; zero means no expiration.</summary>
        static virtual int TimeToLive => 0;
        /// <summary>Gets whether the latest notification is retained.</summary>
        static virtual bool Retain => false;
        /// <summary>Gets whether publication bypasses the current turn's commit.</summary>
        static virtual bool ImmediatePublish => false;
        /// <summary>Creates the notifications to publish when this node executes.</summary>
        Task<IEnumerable<TEvent>> GenerateEventsAsync();
    }
}
