using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IAcceptEventBase<out TReturn>
    {
        #region AddAcceptEventAction
        TReturn AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction<TEvent> buildAction = null);
        
        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent>(AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent>(AcceptEventActionDelegateAsync<TEvent> actionAsync, AcceptEventActionBuildAction<TEvent> buildAction = null)
            => AddAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, actionAsync, buildAction);
        
        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent, TAcceptEventAction>(AcceptEventActionBuildAction<TEvent, TAcceptEventAction> buildAction = null)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => AddAcceptEventAction<TEvent, TAcceptEventAction>(ActivityNode<TAcceptEventAction>.Name, buildAction);

        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent, TAcceptEventAction>(string actionNodeName, AcceptEventActionBuildAction<TEvent, TAcceptEventAction> buildAction = null)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
        {
            var result = AddAcceptEventAction<TEvent>(
                actionNodeName,
                async c =>
                {
                    var action = await ((BaseContext)c).NodeScope.GetAcceptEventActionAsync<TEvent, TAcceptEventAction>(c);

                    InputTokens.TokensHolder.Value = ((ActionContext)c).InputTokens;
                    OutputTokens.TokensHolder.Value = ((ActionContext)c).OutputTokens;

                    ActivityNodeContextAccessor.Context.Value = c;
                    await action.ExecuteAsync(c.Event, c.CancellationToken);
                    ActivityNodeContextAccessor.Context.Value = null;
                },
                b =>
                {
                    var nodeBuilder = (NodeBuilder)b;
                    buildAction?.Invoke(new AcceptEventNodeBuilder<TEvent, TAcceptEventAction>(nodeBuilder.Node, nodeBuilder.ActivityBuilder));
                }
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.AcceptEventNodeTypeAddedAsync<TEvent, TAcceptEventAction>(graph.Name, graph.Version, actionNodeName));

            return result;
        }
        #endregion
        
        #region AddTimeEventAction
        TReturn AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new();
        
        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventNodeBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => AddTimeEventAction<TTimeEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent>(ActionDelegateAsync actionAsync, TimeEventNodeBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => AddTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, c => actionAsync(c), buildAction);

        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent>(TimeEventNodeBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => AddTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, c => Task.CompletedTask, buildAction);
        
        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent, TTimeEventAction>(TimeEventNodeBuildAction<TTimeEventAction> buildAction = null)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : class, ITimeEventActionNode
            => AddTimeEventAction<TTimeEvent, TTimeEventAction>(ActivityNode<TTimeEventAction>.Name, buildAction);

        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent, TTimeEventAction>(string actionNodeName, TimeEventNodeBuildAction<TTimeEventAction> buildAction = null)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : class, ITimeEventActionNode
            => AddTimeEventAction<TTimeEvent>(
                actionNodeName,
                async c =>
                {
                    var action = await ((BaseContext)c).NodeScope.GetTimeEventActionAsync<TTimeEventAction>(c);

                    InputTokens.TokensHolder.Value = ((ActionContext)c).InputTokens;
                    OutputTokens.TokensHolder.Value = ((ActionContext)c).OutputTokens;

                    ActivityNodeContextAccessor.Context.Value = c;
                    await action.ExecuteAsync(c.CancellationToken);
                    ActivityNodeContextAccessor.Context.Value = null;
                },
                b =>
                {
                    var nodeBuilder = (NodeBuilder)b;
                    buildAction?.Invoke(new TimeEventNodeBuilder<TTimeEventAction>(nodeBuilder.Node, nodeBuilder.ActivityBuilder));
                }
            );
        #endregion
    }
}
