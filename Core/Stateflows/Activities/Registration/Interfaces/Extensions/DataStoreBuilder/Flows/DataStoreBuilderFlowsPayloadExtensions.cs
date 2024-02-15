using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class DataStoreBuilderFlowsPayloadExtensions
    {
        public static void AddDataFlow<TTokenPayload>(this IDataStoreBuilder builder, string targetNodeName, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
