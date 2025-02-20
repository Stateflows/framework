using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;
using Stateflows.Common.Context.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;

namespace Stateflows.Actions.Context.Classes
{
    public class ActionContext : IActionContext, IBehaviorLocator
    {
        BehaviorId IBehaviorContext.Id => Context.Id;

        private readonly RootContext Context;

        public readonly List<TokenHolder> OutputTokens = new List<TokenHolder>();
        
        public readonly List<TokenHolder> InputTokens = new List<TokenHolder>();

        private readonly IServiceProvider ServiceProvider;

        public ActionId Id => Context.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(Id, Context.Context, this, ServiceProvider.GetRequiredService<NotificationsHub>());

        public ActionContext(RootContext context, IServiceProvider serviceProvider, IEnumerable<TokenHolder> tokens)
        {
            Context = context;
            ServiceProvider = serviceProvider;
            Values = new ContextValuesCollection(context.GlobalValues);
            if (tokens != null)
            {
                InputTokens.AddRange(tokens);
            }
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
            => InputTokens.OfType<TokenHolder<TToken>>().Select(tokenHolder => tokenHolder.Payload);

        public void Output<TToken>(TToken token)
            => OutputTokens.Add(token.ToTokenHolder());

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            => OutputTokens.AddRange(tokens.Select(token => token.ToTokenHolder()));
    }
}