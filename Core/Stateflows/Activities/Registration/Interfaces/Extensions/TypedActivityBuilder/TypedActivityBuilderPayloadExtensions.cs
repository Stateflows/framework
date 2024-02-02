using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class TypedActivityBuilderPayloadExtensions
    {
        public static ITypedActivityBuilder AddAcceptEventAction<TEventPayload>(this ITypedActivityBuilder builder, string actionNodeName, AcceptEventActionDelegateAsync<Event<TEventPayload>> eventActionAsync, AcceptEventActionBuildAction buildAction = null)
            => builder.AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction);
    }
}
