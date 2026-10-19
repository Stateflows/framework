using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Context;
using Stateflows.Common.Context.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Entities;
using Stateflows.Utils;

namespace Stateflows.Actions.Context.Classes
{
    internal class ActionContext : BehaviorActionContext,
        IActionContext,
        IBehaviorLocator,
        IStateflowsContextProvider,
        IParentBehaviorContext,
        IOwnerBehaviorContext
    {
        BehaviorId IBehaviorContext.Id => RootContext.Id;

        internal readonly RootContext RootContext;
        public List<TokenHolder> OutputTokens { get; } = [];
        public List<TokenHolder> InputTokens { get; } = [];

        public ActionId Id => RootContext.Id;
        
        private IStateflowsSubscriber? subscriber;
        private IStateflowsSubscriber Subscriber
            => subscriber ??= ServiceProvider.GetRequiredService<IStateflowsSubscriber>();

        internal ActionContext(RootContext context, IServiceProvider serviceProvider, IEnumerable<TokenHolder>? tokens)
            : base(context.Context, serviceProvider)
        {
            RootContext = context;
            // Values = new ValuesStorage(
            //     string.Empty,
            //     RootContext.Id,
            //     ServiceProvider.GetRequiredService<IStateflowsLock>(),
            //     ServiceProvider.GetRequiredService<IStateflowsValueStorage>()
            // );
            Values = new ContextValuesCollection(context.Context.GlobalValues);
            
            if (tokens != null)
            {
                InputTokens.AddRange(tokens);
            }
        }

        public IContextValues Values { get; }

        public bool IsEmbedded => Context.ContextOwnerId != null;

        private IBehaviorLocator? behaviorLocator;
        private IBehaviorLocator BehaviorLocator
            => behaviorLocator ??= ServiceProvider.GetRequiredService<IBehaviorLocator>();

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(id, out behavior);

        public IEnumerable<TToken> GetTokensOfType<TToken>()
            => InputTokens.OfType<TokenHolder<TToken>>().Select(tokenHolder => tokenHolder.Payload);
        
        public bool HasTokensOfType<TToken>()
            => InputTokens.OfType<TokenHolder<TToken>>().Any();

        public void Output<TToken>(TToken token)
            => OutputTokens.Add(token.ToTokenHolder());

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            => OutputTokens.AddRange(tokens.Select(token => token.ToTokenHolder()));

        public StateflowsContext Context => RootContext.Context;
        public CancellationToken CancellationToken => CancellationTokenSource.Token;
        BehaviorId IParentBehaviorContext.Id => Context.ContextParentId!.Value;

        BehaviorId IOwnerBehaviorContext.Id => Context.ContextOwnerId!.Value;

        private void PublishRange<TNotification>(BehaviorId behaviorId, IEnumerable<TNotification> notifications, IDictionary<string, EventHeader>? headers = null)
            => RootContext.Executor.PendingTasks.Add(Subscriber.PublishRangeAsync(behaviorId, notifications, Context, headers));

        void IPublishes<IBehaviorContext>.PublishRange<TNotification>(IEnumerable<TNotification> notifications, IDictionary<string, EventHeader>? headers)
            => PublishRange(Context.Id, notifications, headers);

        void IPublishes<IParentBehaviorContext>.PublishRange<TNotification>(IEnumerable<TNotification> notifications, IDictionary<string, EventHeader>? headers)
            => PublishRange(Context.ContextParentId!.Value, notifications, headers);

        void IPublishes<IOwnerBehaviorContext>.PublishRange<TNotification>(IEnumerable<TNotification> notifications, IDictionary<string, EventHeader>? headers)
            => PublishRange(Context.ContextOwnerId!.Value, notifications, headers);

        private async Task<bool> TryMutateAsync<TMutationEvent>(BehaviorId behaviorId, string entityName, TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers)
        {
            var entityId = new EntityId($"{behaviorId.Name}.entity.{entityName}", behaviorId.Instance);
            
            headers ??= new Dictionary<string, EventHeader>();
            headers[nameof(BehaviorEmbedding)] = new BehaviorEmbedding
            {
                OwnerId = behaviorId,
                ParentId = behaviorId
            };

            if (TryLocateBehavior(entityId, out var entity))
            {
                return (await entity.SendAsync(mutationEvent, headers)).Status == EventStatus.Consumed;
            }
            
            return false;
        }
        
        Task<bool> IEntityOperations<IBehaviorContext>.TryMutateAsync<TMutationEvent>(string entityName, TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers)
            => TryMutateAsync(Id, entityName, mutationEvent, headers);

        async Task<bool> IEntityOperations<IParentBehaviorContext>.TryMutateAsync<TMutationEvent>(string entityName, TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers)
            => Context.ContextParentId.HasValue && await TryMutateAsync(Context.ContextParentId!.Value, entityName, mutationEvent, headers);

        async Task<bool> IEntityOperations<IOwnerBehaviorContext>.TryMutateAsync<TMutationEvent>(string entityName, TMutationEvent mutationEvent,
            IDictionary<string, EventHeader>? headers)
            => Context.ContextOwnerId.HasValue && await TryMutateAsync(Context.ContextOwnerId!.Value, entityName, mutationEvent, headers);

        private async Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(BehaviorId behaviorId, string entityName, IDictionary<string, EventHeader>? headers)
        {
            var entityId = new EntityId($"{behaviorId.Name}.entity.{entityName}", behaviorId.Instance);
            
            headers ??= new Dictionary<string, EventHeader>();
            headers[nameof(BehaviorEmbedding)] = new BehaviorEmbedding
            {
                OwnerId = behaviorId,
                ParentId = behaviorId
            };

            if (TryLocateBehavior(entityId, out var entity))
            {
                var result = await entity.RequestAsync(new ProjectionRequest<TProjection>(), headers);
                return (result.Status == EventStatus.Consumed, result.Response);
            }
            
            return (false, default);
        }

        Task<(bool Success, TProjection Projection)> IEntityOperations<IBehaviorContext>.TryGetProjectionAsync<TProjection>(string entityName, IDictionary<string, EventHeader>? headers)
            => TryGetProjectionAsync<TProjection>(Id, entityName, headers);

        async Task<(bool Success, TProjection Projection)> IEntityOperations<IParentBehaviorContext>.TryGetProjectionAsync<TProjection>(string entityName, IDictionary<string, EventHeader>? headers)
            => Context.ContextParentId.HasValue
                ? await TryGetProjectionAsync<TProjection>(Context.ContextParentId!.Value, entityName, headers)
                : (false, default);
        
        async Task<(bool Success, TProjection Projection)> IEntityOperations<IOwnerBehaviorContext>.TryGetProjectionAsync<TProjection>(string entityName, IDictionary<string, EventHeader>? headers)
            => Context.ContextOwnerId.HasValue
                ? await TryGetProjectionAsync<TProjection>(Context.ContextOwnerId!.Value, entityName, headers)
                : (false, default);

        private async Task<bool> TrySetFieldAsync<T>(BehaviorId behaviorId, string entityName, string fieldName, T fieldValue, IDictionary<string, EventHeader>? headers)
        {
            var entityId = new EntityId($"{behaviorId.Name}.entity.{entityName}", behaviorId.Instance);
            
            headers ??= new Dictionary<string, EventHeader>();
            headers[nameof(BehaviorEmbedding)] = new BehaviorEmbedding
            {
                OwnerId = behaviorId,
                ParentId = behaviorId
            };

            if (TryLocateBehavior(entityId, out var entity))
            {
                return (await entity.SendAsync(new FieldState<T> { Name = fieldName, Value = fieldValue }, headers)).Status == EventStatus.Consumed;
            }
            
            return false;
        }
        
        Task<bool> IEntityOperations<IBehaviorContext>.TrySetFieldAsync<T>(string entityName, string fieldName, T fieldValue, IDictionary<string, EventHeader>? headers)
            => TrySetFieldAsync(Id, entityName, fieldName, fieldValue, headers);

        async Task<bool> IEntityOperations<IParentBehaviorContext>.TrySetFieldAsync<T>(string entityName, string fieldName, T fieldValue, IDictionary<string, EventHeader>? headers)
            => Context.ContextParentId.HasValue && await TrySetFieldAsync(Context.ContextParentId!.Value, entityName, fieldName, fieldValue, headers);

        async Task<bool> IEntityOperations<IOwnerBehaviorContext>.TrySetFieldAsync<T>(string entityName, string fieldName, T fieldValue,
            IDictionary<string, EventHeader>? headers)
            => Context.ContextOwnerId.HasValue && await TrySetFieldAsync(Context.ContextOwnerId!.Value, entityName, fieldName, fieldValue, headers);

        private async Task<(bool Success, T Field)> TryGetFieldAsync<T>(string entityName, string fieldName, BehaviorId behaviorId, IDictionary<string, EventHeader>? headers)
        {
            var entityId = new EntityId($"{behaviorId.Name}.entity.{entityName}", behaviorId.Instance);
            
            headers ??= new Dictionary<string, EventHeader>();
            headers[nameof(BehaviorEmbedding)] = new BehaviorEmbedding
            {
                OwnerId = behaviorId,
                ParentId = behaviorId
            };

            if (TryLocateBehavior(entityId, out var entity))
            {
                var result = await entity.RequestAsync(new FieldStateRequest<T> { Name = fieldName }, headers);
                return (result.Status == EventStatus.Consumed, result.Response is not null ? result.Response.Value : default);
            }
            
            return (false, default);
        }

        Task<(bool Success, T Field)> IEntityOperations<IBehaviorContext>.TryGetFieldAsync<T>(string entityName, string fieldName, IDictionary<string, EventHeader>? headers)
            => TryGetFieldAsync<T>(entityName, fieldName, Id, headers);

        async Task<(bool Success, T Field)> IEntityOperations<IParentBehaviorContext>.TryGetFieldAsync<T>(string entityName, string fieldName, IDictionary<string, EventHeader>? headers)
            => Context.ContextParentId.HasValue
                ? await TryGetFieldAsync<T>(entityName, fieldName, Context.ContextParentId!.Value, headers)
                : (false, default);
        
        async Task<(bool Success, T Field)> IEntityOperations<IOwnerBehaviorContext>.TryGetFieldAsync<T>(string entityName, string fieldName, IDictionary<string, EventHeader>? headers)
            => Context.ContextOwnerId.HasValue
                ? await TryGetFieldAsync<T>(entityName, fieldName, Context.ContextOwnerId!.Value, headers)
                : (false, default);

        private Task SubscribeAsync<TNotification>(BehaviorId subscribeeBehaviorId, BehaviorId behaviorId)
            => Subscriber.SubscribeAsync<TNotification>(subscribeeBehaviorId, behaviorId);

        Task ISubscriptions<IBehaviorContext>.SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => SubscribeAsync<TNotification>(Id, behaviorId);

        Task ISubscriptions<IParentBehaviorContext>.SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => SubscribeAsync<TNotification>(Context.ContextParentId!.Value, behaviorId);

        Task ISubscriptions<IOwnerBehaviorContext>.SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => SubscribeAsync<TNotification>(Context.ContextOwnerId!.Value, behaviorId);

        private Task UnsubscribeAsync<TNotification>(BehaviorId subscribeeBehaviorId, BehaviorId behaviorId)
            => Subscriber.UnsubscribeAsync<TNotification>(subscribeeBehaviorId, behaviorId);

        Task ISubscriptions<IBehaviorContext>.UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => UnsubscribeAsync<TNotification>(Id, behaviorId);

        Task ISubscriptions<IParentBehaviorContext>.UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => UnsubscribeAsync<TNotification>(Context.ContextParentId!.Value, behaviorId);

        Task ISubscriptions<IOwnerBehaviorContext>.UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => UnsubscribeAsync<TNotification>(Context.ContextOwnerId!.Value, behaviorId);

        private void Send<TEvent>(BehaviorId behaviorId, TEvent @event, IDictionary<string, EventHeader>? headers = null)
        {
            if (TryLocateBehavior(behaviorId, out var behavior))
            {
                _ = behavior.SendAsync(@event, headers);
            }
        }

        void ISends<IBehaviorContext>.Send<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers)
            => Send<TEvent>(Id, @event, headers);

        void ISends<IParentBehaviorContext>.Send<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers)
            => Send<TEvent>(Context.ContextParentId!.Value, @event, headers);

        void ISends<IOwnerBehaviorContext>.Send<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers)
            => Send<TEvent>(Context.ContextOwnerId!.Value, @event, headers);

        public void PassTokensOfTypeOn<TToken>()
            => OutputTokens.AddRange(InputTokens.OfTokenType<TToken>());

        public void PassAllTokensOn()
            => OutputTokens.AddRange(InputTokens);

    }
}