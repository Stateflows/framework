using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class ReactiveStructuredActivityBuilderPayloadExtensions
    {
        public static IReactiveStructuredActivityBuilder AddAcceptEventAction<TEventPayload>(this IReactiveStructuredActivityBuilder builder, string actionNodeName, AcceptEventActionDelegateAsync<Event<TEventPayload>> eventActionAsync, AcceptEventActionBuildAction buildAction = null)
            => builder.AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction);
    }
}
