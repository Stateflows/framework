using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class StructuredActivityBuilderTypedPayloadExtensions
    {
        #region AddSendEventAction
        public static IStructuredActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IStructuredActivityBuilder builder, SendEventActionBuilderAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<TEventPayload, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static IStructuredActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IStructuredActivityBuilder builder, string actionNodeName, SendEventActionBuilderAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<Event<TEventPayload>, TSendEventAction>(actionNodeName, buildAction);
        #endregion

        #region AddParallelActivity
        public static IStructuredActivityBuilder AddParallelActivity<TTokenPayload, TStructuredActivity>(this IStructuredActivityBuilder builder, ParallelActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddParallelActivity<TTokenPayload, TStructuredActivity>(ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddParallelActivity<TTokenPayload, TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, ParallelActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddParallelActivity<Token<TTokenPayload>, TStructuredActivity>(structuredActivityName, buildAction);
        #endregion

        #region AddIterativeActivity
        public static IStructuredActivityBuilder AddIterativeActivity<TTokenPayload, TStructuredActivity>(this IStructuredActivityBuilder builder, IterativeActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddIterativeActivity<TTokenPayload, TStructuredActivity>(ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddIterativeActivity<TTokenPayload, TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, IterativeActivityBuilderAction buildAction = null)
            where TStructuredActivity : StructuredActivity<Token<TTokenPayload>>
            => builder.AddIterativeActivity<Token<TTokenPayload>, TStructuredActivity>(structuredActivityName, buildAction);
        #endregion
    }
}
