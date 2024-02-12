using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class StructuredActivityBuilderTypedPayloadExtensions
    {
        #region AddSendEventAction
        public static IStructuredActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IStructuredActivityBuilder builder, SendEventActionBuildAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<TEventPayload, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static IStructuredActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IStructuredActivityBuilder builder, string actionNodeName, SendEventActionBuildAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<Event<TEventPayload>, TSendEventAction>(actionNodeName, buildAction);
        #endregion

        #region AddParallelActivity
        public static IStructuredActivityBuilder AddParallelActivity<TTokenPayload, TStructuredActivity>(this IStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction = null)
            where TStructuredActivity : ParallelActivityNode<Token<TTokenPayload>>
            => builder.AddParallelActivity<TTokenPayload, TStructuredActivity>(ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddParallelActivity<TTokenPayload, TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, ParallelActivityBuildAction buildAction = null)
            where TStructuredActivity : ParallelActivityNode<Token<TTokenPayload>>
            => builder.AddParallelActivity<Token<TTokenPayload>, TStructuredActivity>(structuredActivityName, buildAction);
        #endregion

        #region AddIterativeActivity
        public static IStructuredActivityBuilder AddIterativeActivity<TTokenPayload, TStructuredActivity>(this IStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction = null)
            where TStructuredActivity : IterativeActivityNode<Token<TTokenPayload>>
            => builder.AddIterativeActivity<TTokenPayload, TStructuredActivity>(ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IStructuredActivityBuilder AddIterativeActivity<TTokenPayload, TStructuredActivity>(this IStructuredActivityBuilder builder, string structuredActivityName, IterativeActivityBuildAction buildAction = null)
            where TStructuredActivity : IterativeActivityNode<Token<TTokenPayload>>
            => builder.AddIterativeActivity<Token<TTokenPayload>, TStructuredActivity>(structuredActivityName, buildAction);
        #endregion
    }
}
