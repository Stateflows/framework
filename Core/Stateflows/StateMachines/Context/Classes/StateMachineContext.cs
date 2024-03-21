using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;
using Stateflows.Common.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineContext : BaseContext, IStateMachineInspectionContext
    {
        BehaviorId IBehaviorContext.Id => Context.Id;

        public StateMachineId Id => Context.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(Id, Context.Context, this, Context.Executor.ServiceProvider.GetRequiredService<NotificationsHub>());

        public StateMachineContext(RootContext context) : base(context)
        {
            Values = new ContextValues(Context.GlobalValues);
        }

        public IStateMachineInspection Inspection => Context.Executor.Inspector.Inspection;

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => _ = Context.Send(@event);

        public void Publish<TNotification>(TNotification notification)
            where TNotification : Notification, new()
            => _ = Subscriber.PublishAsync(notification);

        public Task<RequestResult<SubscriptionResponse>> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            where TNotification : Notification, new()
            => Subscriber.SubscribeAsync<TNotification>(behaviorId);

        public Task<RequestResult<UnsubscriptionResponse>> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            where TNotification : Notification, new()
            => Subscriber.UnsubscribeAsync<TNotification>(behaviorId);
    }
}
