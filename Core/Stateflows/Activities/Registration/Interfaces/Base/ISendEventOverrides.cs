using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Overrides event-sending nodes in an inherited activity.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
    public interface ISendEventOverrides<out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited node.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseSendEventAction<TEvent>(string actionNodeName, OverridenSendEventActionBuildAction buildAction);
        
        /// <summary>Selects and replaces the matching inherited node.</summary>
        /// <typeparam name="TEvent">Event payload type accepted by this event action node.</typeparam>
        /// <typeparam name="TSendEventAction">Typed node implementation implementing
        /// <see cref="ISendEventActionNode&lt;TEvent&gt;" />.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-send-event-action-node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        TReturn UseSendEventAction<TEvent, TSendEventAction>(OverridenSendEventActionBuildAction buildAction)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
            => UseSendEventAction<TEvent>(ActivityNode<TSendEventAction>.Name, buildAction);
    }
}
