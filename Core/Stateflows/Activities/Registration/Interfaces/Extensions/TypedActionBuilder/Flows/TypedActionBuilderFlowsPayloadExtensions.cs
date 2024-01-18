using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class TypedActionBuilderFlowsPayloadExtensions
    {
        public static ITypedActionBuilder AddDataFlow<TTokenPayload>(this ITypedActionBuilder builder, string targetNodeName, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddTokenFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
