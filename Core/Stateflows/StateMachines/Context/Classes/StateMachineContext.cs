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
            Values = new ContextValuesCollection(Context.GlobalValues);
        }

        public IStateMachineInspection Inspection => Context.Executor.Inspector.Inspection;

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event)

            => _ = Context.Send(@event);

        public void Publish<TNotificationEvent>(TNotificationEvent notification)
            => _ = Subscriber.PublishAsync(Id, notification);

        public Task<SendResult> SubscribeAsync<TNotificationEvent>(BehaviorId behaviorId)
            => Subscriber.SubscribeAsync<TNotificationEvent>(behaviorId);

        public Task<SendResult> UnsubscribeAsync<TNotificationEvent>(BehaviorId behaviorId)
            => Subscriber.UnsubscribeAsync<TNotificationEvent>(behaviorId);
    }
}
