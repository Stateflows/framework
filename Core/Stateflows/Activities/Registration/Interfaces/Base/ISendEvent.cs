using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface ISendEvent<out TReturn>
    {
        TReturn AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction = null);
        
        [DebuggerHidden]
        private static async Task<TResult> GetSendEventAction<TEvent, TSendEventAction, TResult>(Context.Interfaces.IActionContext context, Func<TSendEventAction, Task<TResult>> callback)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
        {
            var action = await ((BaseContext)context).NodeScope.GetSendEventActionAsync<TEvent, TSendEventAction>(context);

            InputTokens.TokensHolder.Value = ((ActionContext)context).InputTokens;
            OutputTokens.TokensHolder.Value = ((ActionContext)context).OutputTokens;

            ActivityNodeContextAccessor.Context.Value = context;
            var result = await callback(action);
            ActivityNodeContextAccessor.Context.Value = null;

            return result;
        }

        [DebuggerHidden]
        public TReturn AddSendEventAction<TEvent, TSendEventAction>(SendEventActionBuildAction buildAction = null)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
            => AddSendEventAction<TEvent, TSendEventAction>(ActivityNode<TSendEventAction>.Name, buildAction);

        [DebuggerHidden]
        public TReturn AddSendEventAction<TEvent, TSendEventAction>(string actionNodeName, SendEventActionBuildAction buildAction = null)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
        {
            var result = AddSendEventAction<TEvent>(
                actionNodeName,
                c => GetSendEventAction<TEvent, TSendEventAction, TEvent>(c, a => a.GenerateEventAsync()),
                c => GetSendEventAction<TEvent, TSendEventAction, BehaviorId>(c, a => a.SelectTargetAsync()),
                buildAction
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.SendEventNodeTypeAddedAsync<TEvent, TSendEventAction>(graph.Name, graph.Version, actionNodeName));

            return result;
        }
    }
}
