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
    }
}
