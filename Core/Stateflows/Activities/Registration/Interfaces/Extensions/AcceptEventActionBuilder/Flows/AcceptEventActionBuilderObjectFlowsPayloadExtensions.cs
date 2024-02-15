using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class AcceptEventActionBuilderObjectFlowsPayloadExtensions
    {
        public static IAcceptEventActionBuilder AddDataFlow<TTokenPayload>(this IAcceptEventActionBuilder builder, string targetNodeName, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
