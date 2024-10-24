using Stateflows.Utils;
using Stateflows.Common;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class StateActionActivityBuilder :
        BaseActivityBuilder,
        IStateActionActivityBuilder,
        IInitializedStateActionActivityBuilder
    {
        public StateActionActivityInitializationBuilderAsync<EventHolder> InitializationBuilder { get; private set; } = null;

        public StateActionActivityBuilder(StateActionActivityBuildAction buildAction)
        {
            buildAction?.Invoke(this);
        }

        IInitializedStateActionActivityBuilder IStateActionInitialization<IInitializedStateActionActivityBuilder>.InitializeWith<TInitializationEvent>(StateActionActivityInitializationBuilderAsync<TInitializationEvent> builderAsync)
        {
            if (builderAsync != null)
            {
                InitializationBuilder = async c => (await builderAsync(c)).ToEventHolder();
            }

            return this;
        }

        public StateActionActivityBuilder AddSubscription<TNotificationEvent>()
        {
            Notifications.Add(typeof(TNotificationEvent));

            return this;
        }

        IStateActionActivityBuilder IStateSubscription<IStateActionActivityBuilder>.AddSubscription<TNotificationEvent>()
            => AddSubscription<TNotificationEvent>();

        IInitializedStateActionActivityBuilder IStateSubscription<IInitializedStateActionActivityBuilder>.AddSubscription<TNotificationEvent>()
            => AddSubscription<TNotificationEvent>();
    }
}
