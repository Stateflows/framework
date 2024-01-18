using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class ActivityBuilderTypedPayloadExtensions
    {
        #region AddAcceptEventAction
        public static IActivityBuilder AddAcceptEventAction<TEventPayload, TAcceptEventAction>(this IActivityBuilder builder, AcceptEventActionBuilderAction buildAction = null)
            where TAcceptEventAction : AcceptEventActionNode<Event<TEventPayload>>
            => builder.AddAcceptEventAction<TEventPayload, TAcceptEventAction>(ActivityNodeInfo<TAcceptEventAction>.Name, buildAction);

        public static IActivityBuilder AddAcceptEventAction<TEventPayload, TAcceptEventAction>(this IActivityBuilder builder, string actionNodeName, AcceptEventActionBuilderAction buildAction = null)
            where TAcceptEventAction : AcceptEventActionNode<Event<TEventPayload>>
            => builder.AddAcceptEventAction<Event<TEventPayload>, TAcceptEventAction>(actionNodeName, buildAction);
        #endregion

        #region AddSendEventAction
        public static IActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IActivityBuilder builder, SendEventActionBuilderAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<TEventPayload, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static IActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IActivityBuilder builder, string actionNodeName, SendEventActionBuilderAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<Event<TEventPayload>, TSendEventAction>(actionNodeName, buildAction);
        #endregion

        #region AddParallelActivity
        public static IActivityBuilder AddParallelActivity<TTokenPayload, TStructuredActivity>(this IActivityBuilder builder, ParallelActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddParallelActivity<TTokenPayload, TStructuredActivity>(ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IActivityBuilder AddParallelActivity<TTokenPayload, TStructuredActivity>(this IActivityBuilder builder, string structuredActivityName, ParallelActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddParallelActivity<Token<TTokenPayload>, TStructuredActivity>(structuredActivityName, buildAction);
        #endregion

        #region AddIterativeActivity
        public static IActivityBuilder AddIterativeActivity<TTokenPayload, TStructuredActivity>(this IActivityBuilder builder, IterativeActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddIterativeActivity<TTokenPayload, TStructuredActivity>(ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IActivityBuilder AddIterativeActivity<TTokenPayload, TStructuredActivity>(this IActivityBuilder builder, string structuredActivityName, IterativeActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddIterativeActivity<Token<TTokenPayload>, TStructuredActivity>(structuredActivityName, buildAction);
        #endregion
    }
}
