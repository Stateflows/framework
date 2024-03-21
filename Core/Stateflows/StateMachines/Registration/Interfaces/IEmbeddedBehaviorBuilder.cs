using Stateflows.Common;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IEmbeddedBehaviorBuilder
    {
        IEmbeddedBehaviorBuilder AddForwardedEvent<TEvent>(ForwardedEventBuildAction<TEvent> buildAction = null)
            where TEvent : Event, new();

        IEmbeddedBehaviorBuilder AddSubscription<TNotification>()
            where TNotification : Notification, new();
    }
}
