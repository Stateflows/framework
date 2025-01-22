using Stateflows.Common;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class TransitionActivityBuilder<TEvent> :
        BaseActivityBuilder,
        ITransitionActivityBuilder<TEvent>,
        IInitializedTransitionActivityBuilder<TEvent>

    {
        public TransitionActivityInitializationBuilderAsync<TEvent, EventHolder> InitializationBuilder { get; private set; } = null;

        public TransitionActivityBuilder(TransitionActivityBuildAction<TEvent> buildAction)
        {
            buildAction?.Invoke(this);
        }

        IInitializedTransitionActivityBuilder<TEvent> ITransitionInitialization<TEvent, IInitializedTransitionActivityBuilder<TEvent>>.InitializeWith<TInitializationEvent>(TransitionActivityInitializationBuilderAsync<TEvent, TInitializationEvent> builderAsync)
        {
            if (builderAsync != null)
            {
                InitializationBuilder = async c => await builderAsync(c);
            }

            return this;
        }

        public TransitionActivityBuilder<TEvent> AddSubscription<TNotification>()
        {
            Notifications.Add(typeof(TNotification));

            return this;
        }

        ITransitionActivityBuilder<TEvent> ISubscription<ITransitionActivityBuilder<TEvent>>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IInitializedTransitionActivityBuilder<TEvent> ISubscription<IInitializedTransitionActivityBuilder<TEvent>>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();
    }
}
