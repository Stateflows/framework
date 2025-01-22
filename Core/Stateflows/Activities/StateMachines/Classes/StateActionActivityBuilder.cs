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

        public StateActionActivityBuilder AddSubscription<TNotification>()
        {
            Notifications.Add(typeof(TNotification));

            return this;
        }

        IStateActionActivityBuilder IStateSubscription<IStateActionActivityBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IInitializedStateActionActivityBuilder IStateSubscription<IInitializedStateActionActivityBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();
    }
}
