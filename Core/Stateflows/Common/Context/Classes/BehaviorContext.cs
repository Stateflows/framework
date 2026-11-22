using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Engine;
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
        private readonly PendingTasks pendingTasks = PendingTasksScope.RequireCurrent();

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

        private void PublishRange<TNotification>(BehaviorId behaviorId, IEnumerable<TNotification> notifications, IDictionary<string, EventHeader>? headers = null)
            => pendingTasks.Add(Subscriber.PublishRangeAsync(behaviorId, notifications, Context, headers));

        void IPublishes<IBehaviorContext>.PublishRange<TNotification>(IEnumerable<TNotification> notifications, IDictionary<string, EventHeader>? headers)
            => PublishRange(Id, notifications, headers);

        void IPublishes<IParentBehaviorContext>.PublishRange<TNotification>(IEnumerable<TNotification> notifications, IDictionary<string, EventHeader>? headers)
            => PublishRange(Context.ContextParentId!.Value, notifications, headers);

        void IPublishes<IOwnerBehaviorContext>.PublishRange<TNotification>(IEnumerable<TNotification> notifications, IDictionary<string, EventHeader>? headers)
            => PublishRange(Context.ContextOwnerId!.Value, notifications, headers);

        private Task<bool> TrySetFieldAsync<T>(BehaviorId? behaviorId, string entityName, string fieldName, T fieldValue, IDictionary<string, EventHeader>? headers)
            => EntityOperations.SetFieldAsync(this, behaviorId, entityName, fieldName, fieldValue, headers);
        
        Task<bool> IEntityOperations<IBehaviorContext>.TrySetFieldAsync<T>(string entityName, string fieldName, T fieldValue, IDictionary<string, EventHeader>? headers)
            => TrySetFieldAsync(Id, entityName, fieldName, fieldValue, headers);

        Task<bool> IEntityOperations<IParentBehaviorContext>.TrySetFieldAsync<T>(string entityName, string fieldName, T fieldValue, IDictionary<string, EventHeader>? headers)
            => TrySetFieldAsync(Context.ContextParentId, entityName, fieldName, fieldValue, headers);

        Task<bool> IEntityOperations<IOwnerBehaviorContext>.TrySetFieldAsync<T>(string entityName, string fieldName, T fieldValue,
            IDictionary<string, EventHeader>? headers)
            => TrySetFieldAsync(Context.ContextOwnerId, entityName, fieldName, fieldValue, headers);

        private Task<bool> TryMutateAsync<TMutationEvent>(BehaviorId? behaviorId, string entityName, TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers)
            => EntityOperations.MutateAsync(this, behaviorId, entityName, mutationEvent, headers);
        
        Task<bool> IEntityOperations<IBehaviorContext>.TryMutateAsync<TMutationEvent>(string entityName, TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers)
            => TryMutateAsync(Id, entityName, mutationEvent, headers);

        Task<bool> IEntityOperations<IParentBehaviorContext>.TryMutateAsync<TMutationEvent>(string entityName, TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers)
            => TryMutateAsync(Context.ContextParentId, entityName, mutationEvent, headers);

        Task<bool> IEntityOperations<IOwnerBehaviorContext>.TryMutateAsync<TMutationEvent>(string entityName, TMutationEvent mutationEvent,
            IDictionary<string, EventHeader>? headers)
            => TryMutateAsync(Context.ContextOwnerId, entityName, mutationEvent, headers);

        private Task<(bool Success, T Field)> TryGetFieldAsync<T>(string entityName, string fieldName, BehaviorId? behaviorId, IDictionary<string, EventHeader>? headers)
            => EntityOperations.GetFieldAsync<T>(this, behaviorId, entityName, fieldName, headers);

        Task<(bool Success, T Field)> IEntityOperations<IBehaviorContext>.TryGetFieldAsync<T>(string entityName, string fieldName, IDictionary<string, EventHeader>? headers)
            => TryGetFieldAsync<T>(entityName, fieldName, Id, headers);

        Task<(bool Success, T Field)> IEntityOperations<IParentBehaviorContext>.TryGetFieldAsync<T>(string entityName, string fieldName, IDictionary<string, EventHeader>? headers)
            => TryGetFieldAsync<T>(entityName, fieldName, Context.ContextParentId, headers);
        
        Task<(bool Success, T Field)> IEntityOperations<IOwnerBehaviorContext>.TryGetFieldAsync<T>(string entityName, string fieldName, IDictionary<string, EventHeader>? headers)
            => TryGetFieldAsync<T>(entityName, fieldName, Context.ContextOwnerId, headers);
        
        private Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(BehaviorId? behaviorId, string entityName, IDictionary<string, EventHeader>? headers)
            => EntityOperations.GetProjectionAsync<TProjection>(this, behaviorId, entityName, headers);

        Task<(bool Success, TProjection Projection)> IEntityOperations<IBehaviorContext>.TryGetProjectionAsync<TProjection>(string entityName, IDictionary<string, EventHeader>? headers)
            => TryGetProjectionAsync<TProjection>(Id, entityName, headers);

        Task<(bool Success, TProjection Projection)> IEntityOperations<IParentBehaviorContext>.TryGetProjectionAsync<TProjection>(string entityName, IDictionary<string, EventHeader>? headers)
            => TryGetProjectionAsync<TProjection>(Context.ContextParentId, entityName, headers);
        
        Task<(bool Success, TProjection Projection)> IEntityOperations<IOwnerBehaviorContext>.TryGetProjectionAsync<TProjection>(string entityName, IDictionary<string, EventHeader>? headers)
            => TryGetProjectionAsync<TProjection>(Context.ContextOwnerId, entityName, headers);

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
            var envelope = @event.ToTypedEventHolder(headers);
            BehaviorObservation.Dispatch(behaviorId, envelope, observation =>
            {
                observation.Dispatched = TryLocateBehavior(behaviorId, out var behavior);
                if (observation.Dispatched == true)
                    _ = SendOperations.SendAsync(behavior, envelope);
            });
        }

        void ISends<IBehaviorContext>.Send<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers)
            => Send<TEvent>(Id, @event, headers);

        void ISends<IParentBehaviorContext>.Send<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers)
            => Send<TEvent>(Context.ContextParentId!.Value, @event, headers);

        void ISends<IOwnerBehaviorContext>.Send<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers)
            => Send<TEvent>(Context.ContextOwnerId!.Value, @event, headers);
    }
}
