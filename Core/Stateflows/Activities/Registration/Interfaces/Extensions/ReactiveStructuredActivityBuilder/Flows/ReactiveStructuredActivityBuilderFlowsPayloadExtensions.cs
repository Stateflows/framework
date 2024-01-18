using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class ReactiveStructuredActivityBuilderFlowsPayloadExtensions
    {
        public static IReactiveStructuredActivityBuilder AddDataFlow<TTokenPayload>(this IReactiveStructuredActivityBuilder builder, string targetNodeName, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddTokenFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
