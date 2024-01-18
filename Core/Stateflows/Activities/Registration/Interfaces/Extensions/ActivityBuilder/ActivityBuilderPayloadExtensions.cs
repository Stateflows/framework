using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class ActivityBuilderPayloadExtensions
    {
        public static IActivityBuilder AddAcceptEventAction<TEventPayload>(this IActivityBuilder builder, string actionNodeName, AcceptEventActionDelegateAsync<Event<TEventPayload>> eventActionAsync, AcceptEventActionBuilderAction buildAction = null)
            => builder.AddAcceptEventAction(actionNodeName, eventActionAsync, buildAction);
    }
}
