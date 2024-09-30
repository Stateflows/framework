using Stateflows.Activities.Extensions;
using Stateflows.Common;
using Stateflows.Utils;

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

        IStateActionActivityBuilder ISubscription<IStateActionActivityBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IInitializedStateActionActivityBuilder ISubscription<IInitializedStateActionActivityBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();
    }
}
