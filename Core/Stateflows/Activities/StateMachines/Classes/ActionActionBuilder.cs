using Stateflows.Common;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class ActionActionBuilder :
        BaseEmbeddedBehaviorBuilder,
        IActionActionBuilder,
        IInitializedActionActionBuilder
    {
        public StateActionBehaviorInitializationBuilderAsync<EventHolder> InitializationBuilder { get; private set; } = null;

        public ActionActionBuilder(StateActionActionBuildAction buildAction)
        {
            buildAction?.Invoke(this);
        }

        public ActionActionBuilder AddSubscription<TNotification>()
        {
            Subscriptions.Add(typeof(TNotification));

            return this;
        }

        public ActionActionBuilder AddRelay<TNotification>()
        {
            Relays.Add(typeof(TNotification));

            return this;
        }

        IActionActionBuilder ISubscription<IActionActionBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IInitializedActionActionBuilder ISubscription<IInitializedActionActionBuilder>.AddSubscription<TNotification>()
            => AddSubscription<TNotification>();

        IActionActionBuilder ISubscription<IActionActionBuilder>.AddRelay<TNotification>()
            => AddRelay<TNotification>();

        IInitializedActionActionBuilder ISubscription<IInitializedActionActionBuilder>.AddRelay<TNotification>()
            => AddRelay<TNotification>();
    }
}
