using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class ActionBuilderObjectFlowsPayloadExtensions
    {
        public static IActionBuilder AddDataFlow<TTokenPayload>(this IActionBuilder builder, string targetNodeName, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddTokenFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
