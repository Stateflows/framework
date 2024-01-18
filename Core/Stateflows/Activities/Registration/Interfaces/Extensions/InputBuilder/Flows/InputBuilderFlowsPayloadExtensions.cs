using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class InputBuilderFlowsPayloadExtensions
    {
        public static IInputBuilder AddDataFlow<TTokenPayload>(this IInputBuilder builder, string targetNodeName, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddTokenFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
