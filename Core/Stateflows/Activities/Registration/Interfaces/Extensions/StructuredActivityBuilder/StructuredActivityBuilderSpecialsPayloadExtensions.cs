using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class StructuredActivityBuilderSpecialsPayloadExtensions
    {
        public static IStructuredActivityBuilder AddDataDecision<TTokenPayload>(this IStructuredActivityBuilder builder, string decisionNodeName, DecisionBuilderAction<Token<TTokenPayload>> decisionBuildAction)
            => builder.AddTokenDecision<Token<TTokenPayload>>(decisionNodeName, decisionBuildAction);
    }
}
