using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class ReactiveStructuredActivityBuilderTypedPayloadExtensions
    {
        //#region AddAcceptEventAction
        //public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEventPayload, TAcceptEventAction>(this IReactiveStructuredActivityBuilder builder, AcceptEventActionBuildAction buildAction = null)
        //    where TAcceptEventAction : AcceptEventActionNode<Event<TEventPayload>>
        //    => builder.AddAcceptEventAction<TEventPayload, TAcceptEventAction>(ActivityNodeInfo<TAcceptEventAction>.Name, buildAction);

        //public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEventPayload, TAcceptEventAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionBuildAction buildAction = null)
        //    where TAcceptEventAction : AcceptEventActionNode<Event<TEventPayload>>
        //    => builder.AddAcceptEventAction<Event<TEventPayload>, TAcceptEventAction>(actionNodeName, buildAction);
        //#endregion

        //#region AddSendEventAction
        //public static IReactiveStructuredActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IReactiveStructuredActivityBuilder builder, SendEventActionBuildAction buildAction = null)
        //    where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
        //    => builder.AddSendEventAction<TEventPayload, TSendEventAction>(ActivityNodeInfo<TSendEventAction>.Name, buildAction);

        //public static IReactiveStructuredActivityBuilder AddSendEventAction<TEventPayload, TSendEventAction>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, SendEventActionBuildAction buildAction = null)
        //    where TSendEventAction : SendEventActionNode<Event<TEventPayload>>
        //    => builder.AddSendEventAction<Event<TEventPayload>, TSendEventAction>(actionNodeName, buildAction);
        //#endregion
    }
}
