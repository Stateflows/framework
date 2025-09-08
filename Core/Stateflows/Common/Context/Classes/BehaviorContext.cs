using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;

namespace Stateflows.Common.Context.Classes
{
    internal class BehaviorContext : BaseContext, IBehaviorContext
    {
        public BehaviorId Id => Context.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(Id, Context, this, ServiceProvider.GetRequiredService<INotificationsHub>());

        public BehaviorContext(StateflowsContext context, IServiceProvider serviceProvider)
            : base(context, serviceProvider)
        {
            // Values = new ContextValuesCollection(context.GlobalValues);
            Values = new ValuesStorage(
                string.Empty,
                Context.ContextOwnerId ?? Context.Id,
                ServiceProvider.GetRequiredService<IStateflowsLock>(),
                ServiceProvider.GetRequiredService<IStateflowsValueStorage>()
            );
        }

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            var locator = ServiceProvider.GetService<IBehaviorLocator>();
            if (locator.TryLocateBehavior(Context.ContextOwnerId ?? Id, out var behavior))
            {
                _ = behavior.SendAsync(@event, headers);
            }
        }

        public void Publish<TNotification>(TNotification notification, IEnumerable<EventHeader> headers = null)
        {
            var strictOwnershipHeader = headers?.OfType<StrictOwnership>().FirstOrDefault();
            var strictOwnershipAttribute = typeof(TNotification).GetCustomAttribute<StrictOwnershipAttribute>();
            var id = strictOwnershipHeader != null || strictOwnershipAttribute != null
                ? Id
                : Context.ContextOwnerId ?? Id;
            
            Subscriber.PublishAsync(id, notification, headers).GetAwaiter().GetResult();
        }

        public Task<SendResult> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => _ = Subscriber.SubscribeAsync<TNotification>(behaviorId);

        public Task<SendResult> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => _ = Subscriber.UnsubscribeAsync<TNotification>(behaviorId);
    }
}
