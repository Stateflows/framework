using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;
using Stateflows.Activities.Engine;
using Stateflows.Common.Engine;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityContext : BaseContext, IActivityContext
    {
        BehaviorId IBehaviorContext.Id => Context.Context.ContextOwnerId ?? Context.Id;
        public BehaviorId ActualId => Context.Id;

        public object LockHandle => Context;

        public ActivityId Id => Context.Id;

        private IReadOnlyTree<INodeContext> activeNodes;
        public IReadOnlyTree<INodeContext> ActiveNodes
            => activeNodes ??= Context.Executor.NodesTree.Translate<INodeContext>(node => new NodeContext(node, null, Context, null));
        
        private IStateflowsSubscriber subscriber;
        private IStateflowsSubscriber Subscriber
            => subscriber ??= ServiceProvider.GetRequiredService<IStateflowsSubscriber>();

        public ActivityContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        {
            Values = new ValuesStorage(
                string.Empty,
                Context.Context.ContextOwnerId ?? Context.Id,
                Context.Executor.NodeScope.ServiceProvider.GetRequiredService<IStateflowsLock>(),
                Context.Executor.NodeScope.ServiceProvider.GetRequiredService<IStateflowsValueStorage>()
            );
        }

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null)
            => _ = Context.SendAsync(@event, headers);

        public void Publish<TNotification>(TNotification notification, IDictionary<string, EventHeader> headers = null)
        {
            var strictOwnershipHeader = headers?.Values.OfType<StrictOwnership>().FirstOrDefault();
            var strictOwnershipAttribute = typeof(TNotification).GetCustomAttribute<StrictOwnershipAttribute>();
            var id = strictOwnershipHeader != null || strictOwnershipAttribute != null
                ? (BehaviorId)Id
                : Context.Context.ContextOwnerId ?? Id;
            
            Subscriber.PublishAsync(notification, Context.Context, headers).GetAwaiter().GetResult();
        }

        public bool IsEmbedded => Context.Context.ContextOwnerId != null;

        public Task SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.SubscribeAsync<TNotification>(Context.Context.ContextParentId ?? Context.Context.Id, behaviorId);

        public Task UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.UnsubscribeAsync<TNotification>(Context.Context.ContextParentId ?? Context.Context.Id, behaviorId);
    }
}
