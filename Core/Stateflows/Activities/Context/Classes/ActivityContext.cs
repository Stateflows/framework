using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Inspection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityContext : BaseContext, IActivityInspectionContext
    {
        BehaviorId IBehaviorContext.Id => Context.Id;

        public ActivityId Id => Context.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(Id, Context.Context, this, ServiceProvider.GetRequiredService<NotificationsHub>());

        public ActivityContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        {
            Values = new ContextValues(Context.GlobalValues);
        }

        public IActivityInspection Inspection => Context.Executor.Inspector.Inspection;

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => _ = Context.Send(@event);

        public void Publish<TNotification>(TNotification notification)
            where TNotification : Notification, new()
            => Subscriber.PublishAsync(notification);

        public Task<RequestResult<SubscriptionResponse>> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            where TNotification : Notification, new()
            => Subscriber.SubscribeAsync<TNotification>(behaviorId);

        public Task<RequestResult<UnsubscriptionResponse>> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            where TNotification : Notification, new()
            => Subscriber.UnsubscribeAsync<TNotification>(behaviorId);
    }
}
