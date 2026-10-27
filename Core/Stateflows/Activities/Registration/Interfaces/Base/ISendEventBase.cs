using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds nodes that generate events and send them to selected behavior instances.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
    public interface ISendEventBase<out TReturn>
    {
        /// <summary>Adds an action node that creates an event and sends it to a selected behavior.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="actionAsync">Asynchronous callback invoked when the node activates; it receives the node's action
        /// context.</param>
        /// <param name="targetSelectorAsync">Asynchronous callback returning the BehaviorId of the behavior that receives
        /// the generated event.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
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

        /// <summary>Adds an action node that creates an event and sends it to a selected behavior.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <typeparam name="TSendEventAction">Typed node implementation implementing
        /// <see cref="ISendEventActionNode&lt;TEvent&gt;" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
        [DebuggerHidden]
        public TReturn AddSendEventAction<TEvent, TSendEventAction>(SendEventActionBuildAction buildAction = null)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
            => AddSendEventAction<TEvent, TSendEventAction>(ActivityNode<TSendEventAction>.Name, buildAction);

        /// <summary>Adds an action node that creates an event and sends it to a selected behavior.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <typeparam name="TSendEventAction">Typed node implementation implementing
        /// <see cref="ISendEventActionNode&lt;TEvent&gt;" />.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
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
