using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Common.Context;
using Stateflows.Common.Engine;
using Stateflows.Common.Utilities;

namespace Stateflows.Actions.Context.Classes
{
    internal class ActionContext : IActionContext, IBehaviorLocator, IStateflowsContextProvider
    {
        BehaviorId IBehaviorContext.Id => Context.ContextOwnerId ?? RootContext.Id;

        internal readonly RootContext RootContext;

        public List<TokenHolder> OutputTokens { get; } = [];
        
        public List<TokenHolder> InputTokens { get; } = [];

        public IServiceProvider ServiceProvider { get; }

        public ActionId Id => RootContext.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(
                Id,
                RootContext.Context,
                this,
                ServiceProvider.GetRequiredService<INotificationsHub>(),
                ServiceProvider.GetRequiredService<CommonInterceptor>(),
                ServiceProvider
            );

        public ActionContext(RootContext rootContext, IServiceProvider serviceProvider, IEnumerable<TokenHolder> tokens)
        {
            RootContext = rootContext;
            ServiceProvider = serviceProvider;
            Values = new ValuesStorage(
                string.Empty,
                RootContext.Context.ContextOwnerId ?? RootContext.Id,
                ServiceProvider.GetRequiredService<IStateflowsLock>(),
                ServiceProvider.GetRequiredService<IStateflowsValueStorage>()
            );
            if (tokens != null)
            {
                InputTokens.AddRange(tokens);
            }
        }

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
            => _ = RootContext.Send(@event, headers);

        public void Publish<TNotification>(TNotification notification, IEnumerable<EventHeader> headers = null)
        {
            var strictOwnershipHeader = headers?.OfType<StrictOwnership>().FirstOrDefault();
            var strictOwnershipAttribute = typeof(TNotification).GetCustomAttribute<StrictOwnershipAttribute>();
            var id = strictOwnershipHeader != null || strictOwnershipAttribute != null
                ? (BehaviorId)Id
                : RootContext.Context.ContextOwnerId ?? Id;
            
            Subscriber.PublishAsync(id, notification, headers).GetAwaiter().GetResult();
        }

        public bool IsEmbedded => Context.ContextOwnerId != null;

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

        public StateflowsContext Context => RootContext.Context;

        internal CancellationTokenSource CancellationTokenSource = new();
        public CancellationToken CancellationToken => CancellationTokenSource.Token;
    }
}