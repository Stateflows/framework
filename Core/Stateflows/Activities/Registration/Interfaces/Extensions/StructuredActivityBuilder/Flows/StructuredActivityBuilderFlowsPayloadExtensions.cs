using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class StructuredActivityBuilderFlowsPayloadExtensions
    {
        public static IStructuredActivityBuilder AddDataFlow<TTokenPayload>(this IStructuredActivityBuilder builder, string targetNodeName, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddTokenFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
