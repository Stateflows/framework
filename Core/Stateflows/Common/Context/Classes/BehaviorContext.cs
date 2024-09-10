using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Context.Classes
{
    internal class BehaviorContext : BaseContext, IBehaviorContext
    {
        public BehaviorId Id => Context.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(Id, Context, this, ServiceProvider.GetRequiredService<NotificationsHub>());

        public BehaviorContext(StateflowsContext context, IServiceProvider serviceProvider)
            : base(context, serviceProvider)
        {
            Values = new ContextValuesCollection(Context.GlobalValues);
        }

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event)
        {
            var locator = ServiceProvider.GetService<IBehaviorLocator>();
            if (locator.TryLocateBehavior(Id, out var behavior))
            {
                _ = behavior.SendAsync(@event);
            }
        }

        public void Publish<TNotificationEvent>(TNotificationEvent notification)
            => _ = Subscriber.PublishAsync(Id, notification);

        public Task<SendResult> SubscribeAsync<TNotificationEvent>(BehaviorId behaviorId)
            => _ = Subscriber.SubscribeAsync<TNotificationEvent>(behaviorId);

        public Task<SendResult> UnsubscribeAsync<TNotificationEvent>(BehaviorId behaviorId)
            => _ = Subscriber.UnsubscribeAsync<TNotificationEvent>(behaviorId);
    }
}
