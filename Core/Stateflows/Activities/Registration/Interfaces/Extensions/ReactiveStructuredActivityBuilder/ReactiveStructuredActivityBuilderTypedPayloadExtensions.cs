using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class ReactiveStructuredActivityBuilderTypedPayloadExtensions
    {
        #region AddAcceptEventAction
        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEventPayload, TAcceptEventAction>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionBuildAction buildAction = null)
            where TAcceptEventAction : AcceptEventActionNode<Event<TEventPayload>>
            => builder.AddAcceptEventAction<TEventPayload, TAcceptEventAction>(ActivityNodeInfo<TAcceptEventAction>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEventPayload, TAcceptEventAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction = null)
            where TAcceptEventAction : AcceptEventActionNode<Event<TEventPayload>>
            => builder.AddAcceptEventAction<Event<TEventPayload>, TAcceptEventAction>(actionNodeName, buildAction);
        #endregion

        #region AddSendEventAction
        public static IReactiveStructuredActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IReactiveStructuredActivityBuilder builder, SendEventActionBuildAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<TEventPayload, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, SendEventActionBuildAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<Event<TEventPayload>, TSendEventAction>(actionNodeName, buildAction);
        #endregion

        #region AddParallelActivity
        public static IReactiveStructuredActivityBuilder AddParallelActivity<TTokenPayload, TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, ParallelActivityBuildAction buildAction = null)
            where TStructuredActivity : ParallelActivityNode<Token<TTokenPayload>>
            => builder.AddParallelActivity<TTokenPayload, TStructuredActivity>(ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddParallelActivity<TTokenPayload, TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, string structuredActivityName, ParallelActivityBuildAction buildAction = null)
            where TStructuredActivity : ParallelActivityNode<Token<TTokenPayload>>
            => builder.AddParallelActivity<Token<TTokenPayload>, TStructuredActivity>(structuredActivityName, buildAction);
        #endregion

        #region AddIterativeActivity
        public static IReactiveStructuredActivityBuilder AddIterativeActivity<TTokenPayload, TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, IterativeActivityBuildAction buildAction = null)
            where TStructuredActivity : IterativeActivityNode<Token<TTokenPayload>>
            => builder.AddIterativeActivity<TTokenPayload, TStructuredActivity>(ActivityNodeInfo<TStructuredActivity>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddIterativeActivity<TTokenPayload, TStructuredActivity>(this IReactiveStructuredActivityBuilder builder, string structuredActivityName, IterativeActivityBuildAction buildAction = null)
            where TStructuredActivity : IterativeActivityNode<Token<TTokenPayload>>
            => builder.AddIterativeActivity<Token<TTokenPayload>, TStructuredActivity>(structuredActivityName, buildAction);
        #endregion
    }
}
