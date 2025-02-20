using Stateflows.Common;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class StateActionActionBuilder :
        BaseEmbeddedBehaviorBuilder,
        IStateActionActionBuilder,
        IInitializedStateActionActionBuilder
    {
        public StateActionBehaviorInitializationBuilderAsync<EventHolder> InitializationBuilder { get; private set; } = null;

        public StateActionActionBuilder(StateActionActionBuildAction buildAction)
        {
            buildAction?.Invoke(this);
        }

        public StateActionActionBuilder AddSubscription<TNotification>()
        {
            Notifications.Add(typeof(TNotification));

            return this;
        }

        IStateActionActionBuilder IStateSubscription<IStateActionActionBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IInitializedStateActionActionBuilder IStateSubscription<IInitializedStateActionActionBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();
    }
}
