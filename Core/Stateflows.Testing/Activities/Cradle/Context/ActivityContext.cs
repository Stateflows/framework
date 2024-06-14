using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Common.Interfaces;
using System.Threading.Tasks;

namespace Stateflows.Testing.Activities.Cradle.Context
{
    internal class ActivityContext : IActivityContext
    {
        public ActivityId Id { get; internal set; }

        public object LockHandle => this;

        public IContextValues Values => throw new global::System.NotImplementedException();

        BehaviorId IBehaviorContext.Id => Id;

        public void Publish<TNotification>(TNotification notification) where TNotification : Notification, new()
        {
            throw new global::System.NotImplementedException();
        }

        public void Send<TEvent>(TEvent @event) where TEvent : Event, new()
        {
            throw new global::System.NotImplementedException();
        }

        public Task<RequestResult<SubscriptionResponse>> SubscribeAsync<TNotification>(BehaviorId behaviorId) where TNotification : Notification, new()
        {
            throw new global::System.NotImplementedException();
        }

        public Task<RequestResult<UnsubscriptionResponse>> UnsubscribeAsync<TNotification>(BehaviorId behaviorId) where TNotification : Notification, new()
        {
            throw new global::System.NotImplementedException();
        }
    }
}
