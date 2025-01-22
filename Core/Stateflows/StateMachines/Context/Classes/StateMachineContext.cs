using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;
using Stateflows.Common.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineContext : BaseContext, IStateMachineInspectionContext
    {
        BehaviorId IBehaviorContext.Id => Context.Id;

        public StateMachineId Id => Context.Id;

        private BehaviorSubscriber subscriber;
        private BehaviorSubscriber Subscriber
            => subscriber ??= new BehaviorSubscriber(Id, Context.Context, this, Context.Executor.ServiceProvider.GetRequiredService<NotificationsHub>());

        public StateMachineContext(RootContext context) : base(context)
        {
            Values = new ContextValuesCollection(Context.GlobalValues);
        }

        public IStateMachineInspection Inspection => Context.Executor.Inspector.Inspection;

        public IContextValues Values { get; }

        private IReadOnlyTree<IStateContext> currentState;
        public IReadOnlyTree<IStateContext> CurrentState
            => currentState ??= Context.Executor.VerticesTree.Translate<IStateContext>(vertex => new StateContext(Context.Executor.Graph.AllVertices.GetValueOrDefault(vertex.Identifier), Context));

        public void Send<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
            => _ = Context.SendAsync(@event, headers);

        public void Publish<TNotification>(TNotification notification, IEnumerable<EventHeader> headers = null, int timeToLiveInSeconds = 60)
            => _ = Subscriber.PublishAsync(Id, notification, headers, timeToLiveInSeconds);

        public Task<SendResult> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.SubscribeAsync<TNotification>(behaviorId);

        public Task<SendResult> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            => Subscriber.UnsubscribeAsync<TNotification>(behaviorId);
    }
}
