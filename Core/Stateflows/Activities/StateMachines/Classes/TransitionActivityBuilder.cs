using Stateflows.Common;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class TransitionActivityBuilder<TEvent> :
        BaseEmbeddedBehaviorBuilder,
        ITransitionActivityBuilder<TEvent>,
        IInitializedTransitionActivityBuilder<TEvent>
    {
        public TransitionBehaviorInitializationBuilderAsync<TEvent, object> InitializationBuilder { get; private set; } = null;

        public TransitionActivityBuilder(TransitionActivityBuildAction<TEvent> buildAction)
        {
            buildAction?.Invoke(this);
        }

        IInitializedTransitionActivityBuilder<TEvent> ITransitionInitialization<TEvent, IInitializedTransitionActivityBuilder<TEvent>>.InitializeWith<TInitializationEvent>(TransitionBehaviorInitializationBuilderAsync<TEvent, TInitializationEvent> builderAsync)
        {
            if (builderAsync != null)
            {
                InitializationBuilder = async c => await builderAsync(c);
            }

            return this;
        }

        private TransitionActivityBuilder<TEvent> AddSubscription<TNotification>()
        {
            Subscriptions.Add(typeof(TNotification));

            return this;
        }

        private TransitionActivityBuilder<TEvent> AddRelay<TNotification>()
        {
            Relays.Add(typeof(TNotification));

            return this;
        }

        ITransitionActivityBuilder<TEvent> ISubscription<ITransitionActivityBuilder<TEvent>>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IInitializedTransitionActivityBuilder<TEvent> ISubscription<IInitializedTransitionActivityBuilder<TEvent>>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        ITransitionActivityBuilder<TEvent> ISubscription<ITransitionActivityBuilder<TEvent>>.AddRelay<TNotification>()
            => AddRelay<TNotification>();

        IInitializedTransitionActivityBuilder<TEvent> ISubscription<IInitializedTransitionActivityBuilder<TEvent>>.AddRelay<TNotification>()
            => AddRelay<TNotification>();
    }
}
