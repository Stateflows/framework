using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Entities;

namespace Stateflows.Common.Context.Classes
{
    internal class BehaviorContext : BaseContext,
        IBehaviorContext,
        IParentBehaviorContext,
        IOwnerBehaviorContext
    {
        BehaviorId IBehaviorContext.Id => Context.Id;
        
        BehaviorId IParentBehaviorContext.Id => Context.ContextParentId!.Value;

        BehaviorId IOwnerBehaviorContext.Id => Context.ContextOwnerId!.Value;

        private BehaviorId Id => Context.Id;
        
        private IStateflowsSubscriber? subscriber;

        private IStateflowsSubscriber Subscriber
            => subscriber ??= ServiceProvider.GetRequiredService<IStateflowsSubscriber>();

        public BehaviorContext(StateflowsContext context, IServiceProvider serviceProvider)
            : base(context, serviceProvider)
        {
            // Values = new ValuesStorage(
            //     string.Empty,
            //     Context.Id,
            //     ServiceProvider.GetRequiredService<IStateflowsLock>(),
            //     ServiceProvider.GetRequiredService<IStateflowsValueStorage>()
            // );
            Values = new ContextValuesCollection(Context.GlobalValues);
        }

        public IContextValues Values { get; }
        
        public bool IsEmbedded => Context.ContextOwnerId != null;

        private void Publish<TNotification>(BehaviorId behaviorId, TNotification notification, IDictionary<string, EventHeader> headers = null)
            => Subscriber.PublishAsync(behaviorId, notification, Context, headers).Wait();

        void IPublishes<IBehaviorContext>.Publish<TNotification>(TNotification notification, IDictionary<string, EventHeader> headers)
            => Publish(Id, notification, headers);

        void IPublishes<IParentBehaviorContext>.Publish<TNotification>(TNotification notification, IDictionary<string, EventHeader> headers)
            => Publish(Context.ContextParentId!.Value, notification, headers);

        void IPublishes<IOwnerBehaviorContext>.Publish<TNotification>(TNotification notification, IDictionary<string, EventHeader> headers)
            => Publish(Context.ContextOwnerId!.Value, notification, headers);

        private async Task<bool> TrySetAsync<T>(BehaviorId behaviorId, string fieldName, T fieldValue, IDictionary<string, EventHeader> headers)
        {
            var entityId = new EntityId($"{behaviorId.Name}.entity", behaviorId.Instance);
            if (TryLocateBehavior(entityId, out var entity))
            {
                return (await entity.SendAsync(new FieldState<T> { Name = fieldName, Value = fieldValue}, headers)).Status == EventStatus.Consumed;
            }
            
            return false;
        }
        
        Task<bool> IEntityOperations<IBehaviorContext>.TrySetAsync<T>(string fieldName, T fieldValue, IDictionary<string, EventHeader> headers)
            => TrySetAsync(Id, fieldName, fieldValue, headers);

        async Task<bool> IEntityOperations<IParentBehaviorContext>.TrySetAsync<T>(string fieldName, T fieldValue, IDictionary<string, EventHeader> headers)
            => Context.ContextParentId.HasValue && await TrySetAsync(Context.ContextParentId!.Value, fieldName, fieldValue, headers);

        async Task<bool> IEntityOperations<IOwnerBehaviorContext>.TrySetAsync<T>(string fieldName, T fieldValue,
            IDictionary<string, EventHeader> headers)
            => Context.ContextOwnerId.HasValue && await TrySetAsync(Context.ContextOwnerId!.Value, fieldName, fieldValue, headers);

        private async Task<bool> TryMutateAsync<TMutationEvent>(BehaviorId behaviorId, TMutationEvent mutationEvent, IDictionary<string, EventHeader> headers)
        {
            var entityId = new EntityId($"{behaviorId.Name}.entity", behaviorId.Instance);
            if (TryLocateBehavior(entityId, out var entity))
            {
                return (await entity.SendAsync(mutationEvent, headers)).Status == EventStatus.Consumed;
            }
            
            return false;
        }
        
        Task<bool> IEntityOperations<IBehaviorContext>.TryMutateAsync<TMutationEvent>(TMutationEvent mutationEvent, IDictionary<string, EventHeader> headers)
            => TryMutateAsync(Id, mutationEvent, headers);

        async Task<bool> IEntityOperations<IParentBehaviorContext>.TryMutateAsync<TMutationEvent>(TMutationEvent mutationEvent, IDictionary<string, EventHeader> headers)
            => Context.ContextParentId.HasValue && await TryMutateAsync(Context.ContextParentId!.Value, mutationEvent, headers);

        async Task<bool> IEntityOperations<IOwnerBehaviorContext>.TryMutateAsync<TMutationEvent>(TMutationEvent mutationEvent,
            IDictionary<string, EventHeader> headers)
            => Context.ContextOwnerId.HasValue && await TryMutateAsync(Context.ContextOwnerId!.Value, mutationEvent, headers);

        private async Task<(bool Success, T Field)> TryGetAsync<T>(string fieldName, BehaviorId behaviorId, IDictionary<string, EventHeader> headers)
        {
            var entityId = new EntityId($"{behaviorId.Name}.entity", behaviorId.Instance);
            if (TryLocateBehavior(entityId, out var entity))
            {
                var result = await entity.RequestAsync(new FieldStateRequest<T> { Name = fieldName }, headers);
                return (result.Status == EventStatus.Consumed, result.Response.Value);
            }
            
            return (false, default);
        }

        Task<(bool Success, T Field)> IEntityOperations<IBehaviorContext>.TryGetAsync<T>(string fieldName, IDictionary<string, EventHeader> headers)
            => TryGetAsync<T>(fieldName, Id, headers);

        async Task<(bool Success, T Field)> IEntityOperations<IParentBehaviorContext>.TryGetAsync<T>(string fieldName, IDictionary<string, EventHeader> headers)
            => Context.ContextParentId.HasValue
                ? await TryGetAsync<T>(fieldName, Context.ContextParentId!.Value, headers)
                : (false, default);
        
        async Task<(bool Success, T Field)> IEntityOperations<IOwnerBehaviorContext>.TryGetAsync<T>(string fieldName, IDictionary<string, EventHeader> headers)
            => Context.ContextOwnerId.HasValue
                ? await TryGetAsync<T>(fieldName, Context.ContextOwnerId!.Value, headers)
                : (false, default);
        
        private async Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(BehaviorId behaviorId, IDictionary<string, EventHeader> headers)
        {
            var entityId = new EntityId($"{behaviorId.Name}.entity", behaviorId.Instance);
            if (TryLocateBehavior(entityId, out var entity))
            {
                var result = await entity.RequestAsync(new ProjectionRequest<TProjection>(), headers);
                return (result.Status == EventStatus.Consumed, result.Response);
            }
            
            return (false, default);
        }

        Task<(bool Success, TProjection Projection)> IEntityOperations<IBehaviorContext>.TryGetProjectionAsync<TProjection>(IDictionary<string, EventHeader> headers)
            => TryGetProjectionAsync<TProjection>(Id, headers);

        async Task<(bool Success, TProjection Projection)> IEntityOperations<IParentBehaviorContext>.TryGetProjectionAsync<TProjection>(IDictionary<string, EventHeader> headers)
            => Context.ContextParentId.HasValue
                ? await TryGetProjectionAsync<TProjection>(Context.ContextParentId!.Value, headers)
                : (false, default);
        
        async Task<(bool Success, TProjection Projection)> IEntityOperations<IOwnerBehaviorContext>.TryGetProjectionAsync<TProjection>(IDictionary<string, EventHeader> headers)
            => Context.ContextOwnerId.HasValue
                ? await TryGetProjectionAsync<TProjection>(Context.ContextOwnerId!.Value, headers)
                : (false, default);

        private Task SubscribeAsync<TNotification>(BehaviorId subscribeeBehaviorId, BehaviorId behaviorId)
            => _ = Subscriber.SubscribeAsync<TNotification>(subscribeeBehaviorId, behaviorId);

        Task ISubscriptions<IBehaviorContext>.SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => SubscribeAsync<TNotification>(Id, behaviorId);

        Task ISubscriptions<IParentBehaviorContext>.SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => SubscribeAsync<TNotification>(Context.ContextParentId!.Value, behaviorId);

        Task ISubscriptions<IOwnerBehaviorContext>.SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => SubscribeAsync<TNotification>(Context.ContextOwnerId!.Value, behaviorId);

        private Task UnsubscribeAsync<TNotification>(BehaviorId subscribeeBehaviorId, BehaviorId behaviorId)
            => _ = Subscriber.UnsubscribeAsync<TNotification>(subscribeeBehaviorId, behaviorId);

        Task ISubscriptions<IBehaviorContext>.UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => UnsubscribeAsync<TNotification>(Id, behaviorId);

        Task ISubscriptions<IParentBehaviorContext>.UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => UnsubscribeAsync<TNotification>(Context.ContextParentId!.Value, behaviorId);

        Task ISubscriptions<IOwnerBehaviorContext>.UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => UnsubscribeAsync<TNotification>(Context.ContextOwnerId!.Value, behaviorId);

        private void Send<TEvent>(BehaviorId behaviorId, TEvent @event, IDictionary<string, EventHeader> headers = null)
        {
            if (TryLocateBehavior(behaviorId, out var behavior))
            {
                _ = behavior.SendAsync(@event, headers);
            }
        }

        void ISends<IBehaviorContext>.Send<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers)
            => Send<TEvent>(Id, @event, headers);

        void ISends<IParentBehaviorContext>.Send<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers)
            => Send<TEvent>(Context.ContextParentId!.Value, @event, headers);

        void ISends<IOwnerBehaviorContext>.Send<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers)
            => Send<TEvent>(Context.ContextOwnerId!.Value, @event, headers);
    }
}
