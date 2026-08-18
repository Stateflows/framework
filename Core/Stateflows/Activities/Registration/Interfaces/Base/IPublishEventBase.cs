using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IPublishEventBase<out TReturn>
    {
        TReturn AddPublishEventAction<TEvent>(string actionNodeName, PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuildAction buildAction = null);
        
        [DebuggerHidden]
        private static async Task<TResult> GetPublishEventAction<TEvent, TPublishEventAction, TResult>(Context.Interfaces.IActionContext context, Func<TPublishEventAction, Task<TResult>> callback)
            where TPublishEventAction : class, IPublishEventActionNode<TEvent>
        {
            var action = await ((BaseContext)context).NodeScope.GetPublishEventActionAsync<TEvent, TPublishEventAction>(context);

            InputTokens.TokensHolder.Value = ((ActionContext)context).InputTokens;
            OutputTokens.TokensHolder.Value = ((ActionContext)context).OutputTokens;

            ActivityNodeContextAccessor.Context.Value = context;
            var result = await callback(action);
            ActivityNodeContextAccessor.Context.Value = null;

            return result;
        }

        [DebuggerHidden]
        public TReturn AddPublishEventAction<TEvent, TPublishEventAction>(PublishEventActionBuildAction buildAction = null)
            where TPublishEventAction : class, IPublishEventActionNode<TEvent>
            => AddPublishEventAction<TEvent, TPublishEventAction>(ActivityNode<TPublishEventAction>.Name, buildAction);

        [DebuggerHidden]
        public TReturn AddPublishEventAction<TEvent, TPublishEventAction>(string actionNodeName, PublishEventActionBuildAction buildAction = null)
            where TPublishEventAction : class, IPublishEventActionNode<TEvent>
        {
            var result = AddPublishEventAction<TEvent>(
                actionNodeName,
                c => GetPublishEventAction<TEvent, TPublishEventAction, TEvent>(c, a => a.GenerateEventAsync()),
                buildAction
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.PublishEventNodeTypeAddedAsync<TEvent, TPublishEventAction>(graph.Name, graph.Version, actionNodeName));

            return result;
        }
    }
}
