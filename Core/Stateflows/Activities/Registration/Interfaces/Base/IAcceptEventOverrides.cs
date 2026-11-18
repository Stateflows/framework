using System.Collections.Generic;
using System.Diagnostics;
using Stateflows.Payloads;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Overrides event-accepting and time-event nodes in an inherited activity.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.10.-Accept-Event-Action-Node" />
    public interface IAcceptEventOverrides<out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited node.</summary>
        /// <param name="eventName">Registered payload name accepted or published by this node.</param>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.10.-Accept-Event-Action-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseAcceptEventAction(string eventName, string actionNodeName,
            OverridenAcceptEventActionBuildAction<Payload> buildAction)
            => UseAcceptEventAction(new[] { eventName }, actionNodeName, buildAction);

        /// <summary>Selects and replaces the matching inherited node accepting exactly the specified payload names.</summary>
        /// <param name="eventNames">Registered payload names accepted by the inherited node; matched by set equality.</param>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.10.-Accept-Event-Action-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.6.-Payloads" />
        TReturn UseAcceptEventAction(IReadOnlyList<string> eventNames, string actionNodeName,
            OverridenAcceptEventActionBuildAction<Payload> buildAction)
        {
            PayloadNameSet.Validate(eventNames, nameof(eventNames));
            return UseAcceptEventAction<Payload>(actionNodeName, b =>
            {
                var node = ((NodeBuilder)b).Node;
                if (node.PayloadNames == null || !PayloadNameSet.SetEquals(node.PayloadNames, eventNames))
                    throw new System.InvalidOperationException($"Accept-event node '{actionNodeName}' does not accept '{PayloadNameSet.Key(eventNames)}'.");
                buildAction?.Invoke(b);
            });
        }

        /// <summary>Selects and replaces the matching inherited node.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.10.-Accept-Event-Action-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseAcceptEventAction<TEvent>(string actionNodeName, OverridenAcceptEventActionBuildAction<TEvent> buildAction);

        /// <summary>Selects and replaces the matching inherited node.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.10.-Accept-Event-Action-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        TReturn UseAcceptEventAction<TEvent>(OverridenAcceptEventActionBuildAction<TEvent> buildAction)
            => UseAcceptEventAction<TEvent>(AcceptEventActionNode<TEvent>.Name, buildAction);
        
        /// <summary>Selects and replaces the matching inherited node.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <typeparam name="TAcceptEventAction">Typed node implementation implementing
        /// <see cref="IAcceptEventActionNode&lt;TEvent&gt;" />.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.10.-Accept-Event-Action-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        TReturn UseAcceptEventAction<TEvent, TAcceptEventAction>(OverridenAcceptEventActionBuildAction<TEvent, TAcceptEventAction> buildAction)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
            => UseAcceptEventAction<TEvent>(ActivityNode<TAcceptEventAction>.Name,
                b =>
                {
                    var nodeBuilder = (NodeBuilder)b;
                    buildAction?.Invoke(new AcceptEventNodeBuilder<TEvent, TAcceptEventAction>(nodeBuilder.Node, nodeBuilder.ActivityBuilder));
                });

        /// <summary>Selects and replaces the matching inherited node.</summary>
        /// <typeparam name="TTimeEvent">Time-event type that triggers this node; it must derive from
        /// <see cref="TimeEvent" /> and have a parameterless constructor.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.10.-Accept-Event-Action-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseTimeEventAction<TTimeEvent>(string actionNodeName, OverridenTimeEventNodeBuildAction buildAction)
            where TTimeEvent : TimeEvent, new();
        
        /// <summary>Selects and replaces the matching inherited node.</summary>
        /// <typeparam name="TTimeEvent">Time-event type that triggers this node; it must derive from
        /// <see cref="TimeEvent" /> and have a parameterless constructor.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.10.-Accept-Event-Action-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        TReturn UseTimeEventAction<TTimeEvent>(OverridenTimeEventNodeBuildAction buildAction)
            where TTimeEvent : TimeEvent, new()
            => UseTimeEventAction<TTimeEvent>(TimeEventActionNode<TTimeEvent>.Name, buildAction);
        
        /// <summary>Selects and replaces the matching inherited node.</summary>
        /// <typeparam name="TTimeEvent">Time-event type that triggers this node; it must derive from
        /// <see cref="TimeEvent" /> and have a parameterless constructor.</typeparam>
        /// <typeparam name="TTimeEventAction">Typed node implementation implementing <see cref="ITimeEventActionNode" />.
        /// </typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.10.-Accept-Event-Action-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        TReturn UseTimeEventAction<TTimeEvent, TTimeEventAction>(OverridenTimeEventNodeBuildAction<TTimeEventAction> buildAction)
            where TTimeEvent : TimeEvent, new()
            where TTimeEventAction : class, ITimeEventActionNode
            => UseTimeEventAction<TTimeEvent>(ActivityNode<TTimeEventAction>.Name, 
                b =>
                {
                    var nodeBuilder = (NodeBuilder)b;
                    buildAction?.Invoke(new TimeEventNodeBuilder<TTimeEventAction>(nodeBuilder.Node, nodeBuilder.ActivityBuilder));
                });
    }
}
