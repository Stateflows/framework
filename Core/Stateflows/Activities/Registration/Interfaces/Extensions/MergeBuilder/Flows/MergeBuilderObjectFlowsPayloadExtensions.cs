using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class MergeBuilderObjectFlowsPayloadExtensions
    {
        public static void AddDataFlow<TTokenPayload>(this IMergeBuilder builder, string targetNodeName, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddTokenFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
