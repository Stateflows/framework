using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Context.Classes
{
    internal class BehaviorContext : BaseContext, IBehaviorContext
    {
        public BehaviorId Id => Context.Id;
        public BehaviorId ActualId => Context.Id;
        
        private IStateflowsSubscriber subscriber;
        private IStateflowsSubscriber Subscriber
            => subscriber ??= ServiceProvider.GetRequiredService<IStateflowsSubscriber>();

        public BehaviorContext(StateflowsContext context, IServiceProvider serviceProvider)
            : base(context, serviceProvider)
        {
            Values = new ValuesStorage(
                string.Empty,
                Context.ContextOwnerId ?? Context.Id,
                ServiceProvider.GetRequiredService<IStateflowsLock>(),
                ServiceProvider.GetRequiredService<IStateflowsValueStorage>()
            );
        }

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
        {
            var locator = ServiceProvider.GetService<IBehaviorLocator>();
            if (locator.TryLocateBehavior(Context.ContextParentId ?? Id, out var behavior))
            {
                _ = behavior.SendAsync(@event, headers);
            }
        }

        public void Publish<TNotification>(TNotification notification, IDictionary<string, EventHeader> headers = null)
        {
            var strictOwnershipHeader = headers?.Values.OfType<StrictOwnership>().FirstOrDefault();
            var strictOwnershipAttribute = typeof(TNotification).GetCustomAttribute<StrictOwnershipAttribute>();
            var id = strictOwnershipHeader != null || strictOwnershipAttribute != null
                ? Id
                : Context.ContextOwnerId ?? Id;
            
            _ = Subscriber.PublishAsync(notification, Context, headers);
        }

        
        public bool IsEmbedded => Context.ContextOwnerId != null;

        public Task<SendResult> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => _ = Subscriber.SubscribeAsync<TNotification>(Context.ContextOwnerId ?? Id, behaviorId);

        public Task<SendResult> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => _ = Subscriber.UnsubscribeAsync<TNotification>(Context.ContextOwnerId ?? Id, behaviorId);
    }
}
