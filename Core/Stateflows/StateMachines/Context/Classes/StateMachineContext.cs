using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineContext : BaseContext, IStateMachineContext
    {
        BehaviorId IBehaviorContext.Id => Context.Context.ContextOwnerId ?? Context.Id;
        public BehaviorId ActualId => Context.Id;

        public StateMachineId Id => Context.Id;
        
        private IStateflowsSubscriber subscriber;
        private IStateflowsSubscriber Subscriber
            => subscriber ??= ServiceProvider.GetRequiredService<IStateflowsSubscriber>();
        
        public StateMachineContext(RootContext context) : base(context)
        {
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

        public void Send<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
            => _ = Context.SendAsync(@event, headers);

        public void Publish<TNotification>(TNotification notification, IDictionary<string, EventHeader> headers = null)
        {
            Subscriber.PublishAsync(notification, Context.Context, headers).GetAwaiter().GetResult();
        }

        public bool IsEmbedded => Context.Context.ContextOwnerId != null;

        public IServiceProvider? serviceProvider = null;
        public IServiceProvider ServiceProvider => serviceProvider ?? Context.Executor.ServiceProvider;

        public Task SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.SubscribeAsync<TNotification>(Context.Context.ContextParentId ?? Context.Context.Id, behaviorId);

        public Task UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.UnsubscribeAsync<TNotification>(Context.Context.ContextParentId ?? Context.Context.Id, behaviorId);
    }
}
