using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;
using Stateflows.Common.Context.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Actions.Context
{
    public class ActionContext : IActionContext, IBehaviorLocator
    {
        BehaviorId IBehaviorContext.Id => Context.Id;

        private readonly RootContext Context;

        private readonly IServiceProvider ServiceProvider;

        public ActionId Id => Context.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(Id, Context.Context, this, ServiceProvider.GetRequiredService<NotificationsHub>());

        public ActionContext(RootContext context, IServiceProvider serviceProvider)
        {
            Context = context;
            ServiceProvider = serviceProvider;
            Values = new ContextValuesCollection(context.GlobalValues);
        }

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)

            => _ = Context.Send(@event, headers);

        public void Publish<TNotification>(TNotification notification, IEnumerable<EventHeader> headers = null, int timeToLiveInSeconds = 60)
            => _ = Subscriber.PublishAsync(Id, notification, headers, timeToLiveInSeconds);

        public Task<SendResult> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.SubscribeAsync<TNotification>(behaviorId);

        public Task<SendResult> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.UnsubscribeAsync<TNotification>(behaviorId);

        private IBehaviorLocator behaviorLocator;
        private IBehaviorLocator BehaviorLocator
            => behaviorLocator ??= ServiceProvider.GetService<IBehaviorLocator>();

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(id, out behavior);

        public IEnumerable<TToken> GetTokensOfType<TToken>()
        {
            throw new NotImplementedException();
        }

        public void Output<TToken>(TToken token)
        {
            throw new NotImplementedException();
        }

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
        {
            throw new NotImplementedException();
        }
    }
}