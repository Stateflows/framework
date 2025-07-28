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

        private TransitionActionBuilder<TEvent> AddSubscription<TNotification>()
        {
            Subscriptions.Add(typeof(TNotification));

            return this;
        }

        private TransitionActionBuilder<TEvent> AddRelay<TNotification>()
        {
            Relays.Add(typeof(TNotification));

            return this;
        }

        ITransitionActionBuilder<TEvent> ISubscription<ITransitionActionBuilder<TEvent>>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IInitializedTransitionActionBuilder<TEvent> ISubscription<IInitializedTransitionActionBuilder<TEvent>>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        ITransitionActionBuilder<TEvent> ISubscription<ITransitionActionBuilder<TEvent>>.AddRelay<TNotification>()
            => AddRelay<TNotification>();

        IInitializedTransitionActionBuilder<TEvent> ISubscription<IInitializedTransitionActionBuilder<TEvent>>.AddRelay<TNotification>()
            => AddRelay<TNotification>();
    }
}
