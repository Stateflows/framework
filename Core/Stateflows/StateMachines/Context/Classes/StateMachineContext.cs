using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineContext : BaseContext, IStateMachineContext
    {
        BehaviorId IBehaviorContext.Id => Context.Context.ContextOwnerId ?? Context.Id;

        public StateMachineId Id => Context.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(
                Id,
                Context.Context,
                this,
                ServiceProvider.GetRequiredService<INotificationsHub>(),
                ServiceProvider.GetRequiredService<CommonInterceptor>(),
                ServiceProvider
            );

        public StateMachineContext(RootContext context) : base(context)
        {
            // Values = new ContextValuesCollection(context.GlobalValues);
            Values = new ValuesStorage(
                string.Empty,
                Context.Context.ContextOwnerId ?? Context.Id,
                Context.Executor.ServiceProvider.GetRequiredService<IStateflowsLock>(),
                Context.Executor.ServiceProvider.GetRequiredService<IStateflowsValueStorage>()
            );
        }

        public Task<IStateMachineInspection> GetInspectionAsync()
            => Task.FromResult(Context.Executor.Inspector.Inspection);

        public IContextValues Values { get; }

        private IReadOnlyTree<IStateContext> currentStates;
        public IReadOnlyTree<IStateContext> CurrentStates
            => currentStates ??= Context.Executor.VerticesTree.Translate<IStateContext>(vertex => new StateContext(Context.Executor.Graph.AllVertices.GetValueOrDefault(vertex.Identifier), Context));

        public bool TryGetStateContext(string stateName, out IStateContext stateContext)
        {
            stateContext = null;
            if (Context.Executor.Graph.AllVertices.TryGetValue(stateName, out var vertex))
            {
                stateContext = new StateContext(vertex, Context);

                return true;
            }
            
            return false;
        }

        public void Send<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
            => _ = Context.SendAsync(@event, headers);

        public void Publish<TNotification>(TNotification notification, IEnumerable<EventHeader> headers = null)
        {
            var strictOwnershipHeader = headers?.OfType<StrictOwnership>().FirstOrDefault();
            var strictOwnershipAttribute = typeof(TNotification).GetCustomAttribute<StrictOwnershipAttribute>();
            var id = strictOwnershipHeader != null || strictOwnershipAttribute != null
                ? (BehaviorId)Id
                : Context.Context.ContextOwnerId ?? Id;
            
            Subscriber.PublishAsync(id, notification, headers).GetAwaiter().GetResult();
        }

        public bool IsEmbedded => Context.Context.ContextOwnerId != null;
        // => Subscriber.PublishAsync(Context.Context.ContextOwnerId ?? Id, notification, headers).GetAwaiter().GetResult();

        public IServiceProvider ServiceProvider => Context.Executor.ServiceProvider;

        public Task<SendResult> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.SubscribeAsync<TNotification>(behaviorId);

        public Task<SendResult> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.UnsubscribeAsync<TNotification>(behaviorId);
    }
}
