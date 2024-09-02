using Stateflows.Common;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class StateActionActivityBuilder :
        BaseActivityBuilder,
        IStateActionActivityBuilder,
        IInitializedStateActionActivityBuilder
    {
        public StateActionActivityInitializationBuilderAsync<object> InitializationBuilder { get; private set; } = null;

        public StateActionActivityBuilder(StateActionActivityBuildAction buildAction)
        {
            buildAction?.Invoke(this);
        }

        IInitializedStateActionActivityBuilder IStateActionInitialization<IInitializedStateActionActivityBuilder>.InitializeWith<TInitializationEvent>(StateActionActivityInitializationBuilderAsync<TInitializationEvent> builderAsync)
        {
            if (builderAsync != null)
            {
                InitializationBuilder = async c => await builderAsync(c);
            }

            return this;
        }

        public StateActionActivityBuilder AddSubscription<TNotification>()
            where TNotification : Notification, new()
        {
            Notifications.Add(typeof(TNotification));

            return this;
        }

        IStateActionActivityBuilder ISubscription<IStateActionActivityBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IInitializedStateActionActivityBuilder ISubscription<IInitializedStateActionActivityBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();
    }
}
