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

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityContext : BaseContext, IActivityContext
    {
        BehaviorId IBehaviorContext.Id => Context.Id;

        public object LockHandle => Context;

        public ActivityId Id => Context.Id;

        private IReadOnlyTree<INodeContext> activeNodes;
        public IReadOnlyTree<INodeContext> ActiveNodes
            => activeNodes ??= Context.Executor.NodesTree.Translate<INodeContext>(node => new NodeContext(node, null, Context, null));

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(Id, Context.Context, this, ServiceProvider.GetRequiredService<INotificationsHub>());

        public ActivityContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        {
            // Values = new ContextValuesCollection(context.GlobalValues);
            Values = new ValuesStorage(
                string.Empty,
                Context.Context.ContextOwnerId ?? Context.Id,
                Context.Executor.NodeScope.ServiceProvider.GetRequiredService<IStateflowsLock>(),
                Context.Executor.NodeScope.ServiceProvider.GetRequiredService<IStateflowsValueStorage>()
            );

        }

        public IContextValues Values { get; }

        public void Send<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
            => _ = Context.Send(@event, headers);

        public void Publish<TNotification>(TNotification notification, IEnumerable<EventHeader> headers = null)
        {
            var strictOwnershipHeader = headers?.OfType<StrictOwnership>().FirstOrDefault();
            var strictOwnershipAttribute = typeof(TNotification).GetCustomAttribute<StrictOwnershipAttribute>();
            var id = strictOwnershipHeader != null || strictOwnershipAttribute != null
                ? (BehaviorId)Id
                : Context.Context.ContextOwnerId ?? Id;
            
            Subscriber.PublishAsync(id, notification, headers).GetAwaiter().GetResult();
        }

        public Task<SendResult> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.SubscribeAsync<TNotification>(behaviorId);

        public Task<SendResult> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.UnsubscribeAsync<TNotification>(behaviorId);
    }
}
