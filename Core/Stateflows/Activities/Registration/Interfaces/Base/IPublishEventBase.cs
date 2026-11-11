using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds nodes that generate and publish events.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.12.-Publish-Event-Action-Node" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.12.-Publish-Event-Action-Node" />
        TReturn AddPublishEventAction<TEvent>(string actionNodeName, PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuildAction buildAction = null);

        /// <summary>Adds an action node that creates and publishes multiple notifications.</summary>
        TReturn AddPublishEventAction<TEvent>(string actionNodeName, Func<Context.Interfaces.IActionContext, Task<IEnumerable<TEvent>>> actionAsync, PublishEventActionBuildAction buildAction = null);
        
        [DebuggerHidden]
        private static async Task<IEnumerable<TEvent>> GetPublishEventAction<TEvent, TPublishEventAction>(Context.Interfaces.IActionContext context)
            where TPublishEventAction : class, IPublishEventActionNode<TEvent>
        {
            InputTokens.TokensHolder.Value = ((ActionContext)context).InputTokens;
            OutputTokens.TokensHolder.Value = ((ActionContext)context).OutputTokens;
            ActivityNodeContextAccessor.Context.Value = context;
            try
            {
                var action = await ((BaseContext)context).NodeScope.GetPublishEventActionAsync<TEvent, TPublishEventAction>(context);
                return (await action.GenerateEventsAsync()).ToArray();
            }
            finally
            {
                ActivityNodeContextAccessor.Context.Value = null;
            }
        }

        /// <summary>Adds an action node that creates and publishes an event.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <typeparam name="TPublishEventAction">Typed node implementation implementing
        /// <see cref="IPublishEventActionNode&lt;TEvent&gt;" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.12.-Publish-Event-Action-Node" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.12.-Publish-Event-Action-Node" />
        [DebuggerHidden]
        public TReturn AddPublishEventAction<TEvent, TPublishEventAction>(string actionNodeName, PublishEventActionBuildAction buildAction = null)
            where TPublishEventAction : class, IPublishEventActionNode<TEvent>
        {
            var result = AddPublishEventAction<TEvent>(
                actionNodeName,
                GetPublishEventAction<TEvent, TPublishEventAction>,
                builder =>
                {
                    builder.SetPublishScope(TPublishEventAction.PublishScope)
                        .SetTimeToLive(TPublishEventAction.TimeToLive)
                        .SetRetain(TPublishEventAction.Retain)
                        .SetImmediatePublish(TPublishEventAction.ImmediatePublish);
                    buildAction?.Invoke(builder);
                }
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.PublishEventNodeTypeAddedAsync<TEvent, TPublishEventAction>(graph.Name, graph.Version, actionNodeName));

            return result;
        }
    }
}
