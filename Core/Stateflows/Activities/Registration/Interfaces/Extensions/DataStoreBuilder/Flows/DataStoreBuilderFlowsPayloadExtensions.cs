using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class DataStoreBuilderFlowsPayloadExtensions
    {
        public static void AddDataFlow<TTokenPayload>(this IDataStoreBuilder builder, string targetNodeName, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddTokenFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
