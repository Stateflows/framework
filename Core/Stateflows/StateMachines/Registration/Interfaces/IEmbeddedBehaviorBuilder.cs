namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IEmbeddedBehaviorBuilder
    {
        IEmbeddedBehaviorBuilder AddForwardedEvent<TEvent>(ForwardedEventBuildAction<TEvent> buildAction = null)
;

        IEmbeddedBehaviorBuilder AddSubscription<TNotificationEvent>();
    }
}
