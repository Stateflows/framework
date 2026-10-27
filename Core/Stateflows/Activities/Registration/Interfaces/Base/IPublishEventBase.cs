using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds nodes that generate and publish events.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-publish-event-action-node" />
    public interface IPublishEventBase<out TReturn>
    {
        /// <summary>Adds an action node that creates and publishes an event.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="actionAsync">Asynchronous callback invoked when the node activates; it receives the node's action
        /// context.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-publish-event-action-node" />
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

        /// <summary>Adds an action node that creates and publishes an event.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <typeparam name="TPublishEventAction">Typed node implementation implementing
        /// <see cref="IPublishEventActionNode&lt;TEvent&gt;" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-publish-event-action-node" />
        [DebuggerHidden]
        public TReturn AddPublishEventAction<TEvent, TPublishEventAction>(PublishEventActionBuildAction buildAction = null)
            where TPublishEventAction : class, IPublishEventActionNode<TEvent>
            => AddPublishEventAction<TEvent, TPublishEventAction>(ActivityNode<TPublishEventAction>.Name, buildAction);

        /// <summary>Adds an action node that creates and publishes an event.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <typeparam name="TPublishEventAction">Typed node implementation implementing
        /// <see cref="IPublishEventActionNode&lt;TEvent&gt;" />.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-publish-event-action-node" />
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
