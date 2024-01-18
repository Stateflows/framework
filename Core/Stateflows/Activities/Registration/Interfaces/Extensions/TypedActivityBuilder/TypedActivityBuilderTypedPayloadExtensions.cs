using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class TypedActivityBuilderTypedPayloadExtensions
    {
        #region AddAcceptEventAction
        public static ITypedActivityBuilder AddAcceptEventAction<TEventPayload, TAcceptEventAction>(this ITypedActivityBuilder builder, AcceptEventActionBuilderAction buildAction = null)
            where TAcceptEventAction : AcceptEventActionNode<Event<TEventPayload>>
            => builder.AddAcceptEventAction<TEventPayload, TAcceptEventAction>(ActivityNodeInfo<TAcceptEventAction>.Name, buildAction);

        public static ITypedActivityBuilder AddAcceptEventAction<TEventPayload, TAcceptEventAction>(this ITypedActivityBuilder builder, string actionNodeName, AcceptEventActionBuilderAction buildAction = null)
            where TAcceptEventAction : AcceptEventActionNode<Event<TEventPayload>>
            => builder.AddAcceptEventAction<Event<TEventPayload>, TAcceptEventAction>(actionNodeName, buildAction);
        #endregion

        #region AddSendEventAction
        public static ITypedActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this ITypedActivityBuilder builder, SendEventActionBuilderAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<TEventPayload, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static ITypedActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this ITypedActivityBuilder builder, string actionNodeName, SendEventActionBuilderAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<Event<TEventPayload>, TSendEventAction>(actionNodeName, buildAction);
        #endregion

        #region AddParallelActivity
        public static ITypedActivityBuilder AddParallelActivity<TTokenPayload, TStructuredActivity>(this ITypedActivityBuilder builder, ParallelActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddParallelActivity<TTokenPayload, TStructuredActivity>(ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static ITypedActivityBuilder AddParallelActivity<TTokenPayload, TStructuredActivity>(this ITypedActivityBuilder builder, string structuredActivityName, ParallelActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddParallelActivity<Token<TTokenPayload>, TStructuredActivity>(structuredActivityName, buildAction);
        #endregion

        #region AddIterativeActivity
        public static ITypedActivityBuilder AddIterativeActivity<TTokenPayload, TStructuredActivity>(this ITypedActivityBuilder builder, IterativeActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddIterativeActivity<TTokenPayload, TStructuredActivity>(ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static ITypedActivityBuilder AddIterativeActivity<TTokenPayload, TStructuredActivity>(this ITypedActivityBuilder builder, string structuredActivityName, IterativeActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddIterativeActivity<Token<TTokenPayload>, TStructuredActivity>(structuredActivityName, buildAction);
        #endregion
    }
}
