using Stateflows.Activities.StateMachines.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IEmbeddedBehaviorBuilder : ISubscription<IEmbeddedBehaviorBuilder>
    {
        IEmbeddedBehaviorBuilder AddForwardedEvent<TEvent>(ForwardedEventBuildAction<TEvent> buildAction = null);
    }
}
