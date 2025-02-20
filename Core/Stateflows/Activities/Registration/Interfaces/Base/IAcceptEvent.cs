using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Classes;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IAcceptEvent<out TReturn>
    {
        #region AddAcceptEventAction
        TReturn AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction buildAction = null);
        
        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionBuildAction buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent>(AcceptEventActionBuildAction buildAction)
            => AddAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent>(AcceptEventActionDelegateAsync<TEvent> actionAsync, AcceptEventActionBuildAction buildAction = null)
            => AddAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, actionAsync, buildAction);
        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent, TAcceptEventAction>(AcceptEventActionBuildAction buildAction = null)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => AddAcceptEventAction<TEvent, TAcceptEventAction>(ActivityNode<TAcceptEventAction>.Name, buildAction);

        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent, TAcceptEventAction>(string actionNodeName, AcceptEventActionBuildAction buildAction = null)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => AddAcceptEventAction<TEvent>(
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
                buildAction
            );
        #endregion
        
        #region AddTimeEventAction
        TReturn AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new();
        
        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent>(string actionNodeName, AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => AddTimeEventAction<TTimeEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent>(ActionDelegateAsync actionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => AddTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, c => actionAsync(c), buildAction);

        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent>(AcceptEventActionBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => AddTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, c => Task.CompletedTask, buildAction);
        
        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent, TTimeEventAction>(AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : class, ITimeEventActionNode
            => AddTimeEventAction<TTimeEvent, TTimeEventAction>(ActivityNode<TTimeEventAction>.Name, buildAction);

        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent, TTimeEventAction>(string actionNodeName, AcceptEventActionBuildAction buildAction = null)
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
                buildAction
            );
        #endregion
    }
}
