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
            => subscriber ??= new BehaviorSubscriber(Id, Context, this);

        public BehaviorContext(StateflowsContext context, IServiceProvider serviceProvider)
            : base(context, serviceProvider)
        {
            Values = new ContextValues(Context.GlobalValues);
        }

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event) where TEvent : Event, new()
        {
            var locator = ServiceProvider.GetService<IBehaviorLocator>();
            if (locator.TryLocateBehavior(Id, out var behavior))
            {
                _ = behavior.SendAsync(@event);
            }
        }

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
