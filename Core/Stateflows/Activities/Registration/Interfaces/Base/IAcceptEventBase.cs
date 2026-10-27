using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds event-accepting and time-event action nodes to an activity.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
    public interface IAcceptEventBase<out TReturn>
    {
        /// <summary>Adds an action node that activates for the specified event while the node is active.</summary>
        /// <param name="eventName">Registered structural-event name accepted or published by this node.</param>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="eventActionAsync">Asynchronous callback invoked with the accepted event and node context.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        TReturn AddAcceptEventAction(string eventName, string actionNodeName,
            AcceptEventActionDelegateAsync<StructuralEvent> eventActionAsync,
            AcceptEventActionBuildAction<StructuralEvent> buildAction = null)
        {
            System.ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            System.ArgumentNullException.ThrowIfNull(eventActionAsync);
            ((BaseActivityBuilder)this).AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction, eventName);
            return (TReturn)this;
        }

        /// <summary>Adds an action node that activates for the specified event while the node is active.</summary>
        /// <param name="eventName">Registered structural-event name accepted or published by this node.</param>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        TReturn AddAcceptEventAction(string eventName, string actionNodeName,
            AcceptEventActionBuildAction<StructuralEvent> buildAction = null)
            => AddAcceptEventAction(eventName, actionNodeName, _ => Task.CompletedTask, buildAction);

        #region AddAcceptEventAction
        /// <summary>Adds an action node that activates for the specified event while the node is active.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="eventActionAsync">Asynchronous callback invoked with the accepted event and node context.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        TReturn AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction<TEvent> buildAction = null);
        
        /// <summary>Adds an action node that activates for the specified event while the node is active.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the event-accepting node's options, flows, and exception
        /// handlers. Its builder is scoped to the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        /// <summary>Adds an action node that activates for the specified event while the node is active.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <param name="buildAction">Callback configuring the event-accepting node's options, flows, and exception
        /// handlers. Its builder is scoped to the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent>(AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, c => Task.CompletedTask, buildAction);

        /// <summary>Adds an action node that activates for the specified event while the node is active.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <param name="actionAsync">Asynchronous callback invoked with the accepted event and node context.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent>(AcceptEventActionDelegateAsync<TEvent> actionAsync, AcceptEventActionBuildAction<TEvent> buildAction = null)
            => AddAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, actionAsync, buildAction);
        
        /// <summary>Adds an action node that activates for the specified event while the node is active.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <typeparam name="TAcceptEventAction">Typed node implementation implementing
        /// <see cref="IAcceptEventActionNode&lt;TEvent&gt;" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        [DebuggerHidden]
        public TReturn AddAcceptEventAction<TEvent, TAcceptEventAction>(AcceptEventActionBuildAction<TEvent, TAcceptEventAction> buildAction = null)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => AddAcceptEventAction<TEvent, TAcceptEventAction>(ActivityNode<TAcceptEventAction>.Name, buildAction);

        /// <summary>Adds an action node that activates for the specified event while the node is active.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <typeparam name="TAcceptEventAction">Typed node implementation implementing
        /// <see cref="IAcceptEventActionNode&lt;TEvent&gt;" />.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
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
        /// <summary>Adds an action node triggered by the specified scheduled time event.</summary>
        /// <typeparam name="TTimeEvent">Time-event type that triggers this node; it must derive from
        /// <see cref="TimeEvent" /> and have a parameterless constructor.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="eventActionAsync">Asynchronous callback invoked when the node activates; it receives the node's
        /// action context.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        TReturn AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new();
        
        /// <summary>Adds an action node triggered by the specified scheduled time event.</summary>
        /// <typeparam name="TTimeEvent">Time-event type that triggers this node; it must derive from
        /// <see cref="TimeEvent" /> and have a parameterless constructor.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the time-event node's options, flows, and exception handlers. Its
        /// builder is scoped to the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventNodeBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => AddTimeEventAction<TTimeEvent>(actionNodeName, c => Task.CompletedTask, buildAction);

        /// <summary>Adds an action node triggered by the specified scheduled time event.</summary>
        /// <typeparam name="TTimeEvent">Time-event type that triggers this node; it must derive from
        /// <see cref="TimeEvent" /> and have a parameterless constructor.</typeparam>
        /// <param name="actionAsync">Asynchronous callback invoked when the node activates; it receives the node's action
        /// context.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent>(ActionDelegateAsync actionAsync, TimeEventNodeBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new()
            => AddTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, c => actionAsync(c), buildAction);

        /// <summary>Adds an action node triggered by the specified scheduled time event.</summary>
        /// <typeparam name="TTimeEvent">Time-event type that triggers this node; it must derive from
        /// <see cref="TimeEvent" /> and have a parameterless constructor.</typeparam>
        /// <param name="buildAction">Callback configuring the time-event node's options, flows, and exception handlers. Its
        /// builder is scoped to the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent>(TimeEventNodeBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => AddTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, c => Task.CompletedTask, buildAction);
        
        /// <summary>Adds an action node triggered by the specified scheduled time event.</summary>
        /// <typeparam name="TTimeEvent">Time-event type that triggers this node; it must derive from
        /// <see cref="TimeEvent" /> and have a parameterless constructor.</typeparam>
        /// <typeparam name="TTimeEventAction">Typed node implementation implementing <see cref="ITimeEventActionNode" />.
        /// </typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
        [DebuggerHidden]
        public TReturn AddTimeEventAction<TTimeEvent, TTimeEventAction>(TimeEventNodeBuildAction<TTimeEventAction> buildAction = null)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : class, ITimeEventActionNode
            => AddTimeEventAction<TTimeEvent, TTimeEventAction>(ActivityNode<TTimeEventAction>.Name, buildAction);

        /// <summary>Adds an action node triggered by the specified scheduled time event.</summary>
        /// <typeparam name="TTimeEvent">Time-event type that triggers this node; it must derive from
        /// <see cref="TimeEvent" /> and have a parameterless constructor.</typeparam>
        /// <typeparam name="TTimeEventAction">Typed node implementation implementing <see cref="ITimeEventActionNode" />.
        /// </typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-accept-event-action-node" />
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
