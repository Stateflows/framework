using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class ActivityBuilderTypedPayloadExtensions
    {
        #region AddAcceptEventAction
        public static IActivityBuilder AddAcceptEventAction<TEventPayload, TAcceptEventAction>(this IActivityBuilder builder, AcceptEventActionBuildAction buildAction = null)
            where TAcceptEventAction : AcceptEventActionNode<Event<TEventPayload>>
            => builder.AddAcceptEventAction<TEventPayload, TAcceptEventAction>(ActivityNodeInfo<TAcceptEventAction>.Name, buildAction);

        public static IActivityBuilder AddAcceptEventAction<TEventPayload, TAcceptEventAction>(this IActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction = null)
            where TAcceptEventAction : AcceptEventActionNode<Event<TEventPayload>>
            => builder.AddAcceptEventAction<Event<TEventPayload>, TAcceptEventAction>(actionNodeName, buildAction);
        #endregion

        #region AddSendEventAction
        public static IActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IActivityBuilder builder, SendEventActionBuildAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<TEventPayload, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        public static IActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IActivityBuilder builder, string actionNodeName, SendEventActionBuildAction buildAction = null)
            where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
            => builder.AddSendEventAction<Event<TEventPayload>, TSendEventAction>(actionNodeName, buildAction);
        #endregion
    }
}
