using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class TypedActivityBuilderSpecialsPayloadExtensions
    {
        public static ITypedActivityBuilder AddDataDecision<TTokenPayload>(this ITypedActivityBuilder builder, string decisionNodeName, DecisionBuilderAction<Token<TTokenPayload>> decisionBuildAction)
            => builder.AddTokenDecision<Token<TTokenPayload>>(decisionNodeName, decisionBuildAction);
    }
}
