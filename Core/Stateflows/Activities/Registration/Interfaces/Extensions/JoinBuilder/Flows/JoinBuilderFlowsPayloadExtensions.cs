using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class JoinBuilderFlowsPayloadExtensions
    {
        public static void AddDataFlow<TTokenPayload>(this IJoinBuilder builder, string targetNodeName, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
