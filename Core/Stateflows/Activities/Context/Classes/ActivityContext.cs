using System.Collections.Generic;
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

        public object LockHandle => Context;

        public ActivityId Id => Context.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(Id, Context.Context, this, ServiceProvider.GetRequiredService<NotificationsHub>());

        public ActivityContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        {
            Values = new ContextValuesCollection(Context.GlobalValues);
        }

        public IActivityInspection Inspection => Context.Executor.Inspector.Inspection;

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)

            => _ = Context.Send(@event, headers);

        public void Publish<TNotification>(TNotification notification, IEnumerable<EventHeader> headers = null, int timeToLiveInSeconds = 60)
            => _ = Subscriber.PublishAsync(Id, notification, headers, timeToLiveInSeconds);

        public Task<SendResult> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.SubscribeAsync<TNotification>(behaviorId);

        public Task<SendResult> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.UnsubscribeAsync<TNotification>(behaviorId);
    }
}
