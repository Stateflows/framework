using Stateflows.Common;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class ActionActivityBuilder :
        BaseEmbeddedBehaviorBuilder,
        IActionActivityBuilder,
        IInitializedActionActivityBuilder
    {
        public StateActionBehaviorInitializationBuilderAsync<EventHolder> InitializationBuilder { get; private set; } = null;

        public ActionActivityBuilder(StateActionActivityBuildAction buildAction)
        {
            buildAction?.Invoke(this);
        }

        IInitializedActionActivityBuilder IStateActionInitialization<IInitializedActionActivityBuilder>.InitializeWith<TInitializationEvent>(StateActionBehaviorInitializationBuilderAsync<TInitializationEvent> builderAsync)
        {
            if (builderAsync != null)
            {
                InitializationBuilder = async c => (await builderAsync(c)).ToEventHolder();
            }

            return this;
        }

        public ActionActivityBuilder AddSubscription<TNotification>()
        {
            Subscriptions.Add(typeof(TNotification));

            return this;
        }

        public ActionActivityBuilder AddRelay<TNotification>()
        {
            Relays.Add(typeof(TNotification));

            return this;
        }

        IInitializedActionActivityBuilder ISubscription<IInitializedActionActivityBuilder>.AddRelay<TNotification>()
            => AddRelay<TNotification>();

        IActionActivityBuilder ISubscription<IActionActivityBuilder>.AddRelay<TNotification>()
            => AddRelay<TNotification>();

        IActionActivityBuilder ISubscription<IActionActivityBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IInitializedActionActivityBuilder ISubscription<IInitializedActionActivityBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();
    }
}
