using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Common.Subscription;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityContext : BaseContext, IActivityInspectionContext
    {
        BehaviorId IBehaviorContext.Id => Context.Id;

        public ActivityId Id => Context.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(Id, Context.Context, this);

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

        public void Publish<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => Subscriber.PublishAsync(@event);

        public Task<RequestResult<SubscriptionResponse>> SubscribeAsync<TEvent>(BehaviorId behaviorId)
            where TEvent : Event, new()
            => Subscriber.SubscribeAsync<TEvent>(behaviorId);

        public Task<RequestResult<UnsubscriptionResponse>> UnsubscribeAsync<TEvent>(BehaviorId behaviorId)
            where TEvent : Event, new()
            => Subscriber.UnsubscribeAsync<TEvent>(behaviorId);
    }
}
