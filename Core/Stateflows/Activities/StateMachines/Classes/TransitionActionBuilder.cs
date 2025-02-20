using Stateflows.Common;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class TransitionActionBuilder<TEvent> :
        BaseEmbeddedBehaviorBuilder,
        ITransitionActionBuilder<TEvent>,
        IInitializedTransitionActionBuilder<TEvent>

    {
        public TransitionBehaviorInitializationBuilderAsync<TEvent, EventHolder> InitializationBuilder { get; private set; } = null;

        public TransitionActionBuilder(TransitionActionBuildAction<TEvent> buildAction)
        {
            buildAction?.Invoke(this);
        }

        public TransitionActionBuilder<TEvent> AddSubscription<TNotification>()
        {
            Notifications.Add(typeof(TNotification));

            return this;
        }

        ITransitionActionBuilder<TEvent> ISubscription<ITransitionActionBuilder<TEvent>>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IInitializedTransitionActionBuilder<TEvent> ISubscription<IInitializedTransitionActionBuilder<TEvent>>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();
    }
}
