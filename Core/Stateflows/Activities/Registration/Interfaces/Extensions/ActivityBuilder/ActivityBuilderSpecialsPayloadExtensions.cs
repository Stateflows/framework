using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class ActivityBuilderSpecialsPayloadExtensions
    {
        public static IActivityBuilder AddDataDecision<TTokenPayload>(this IActivityBuilder builder, string decisionNodeName, DecisionBuildAction<Token<TTokenPayload>> decisionBuildAction)
            => builder.AddTokenDecision<Token<TTokenPayload>>(decisionNodeName, decisionBuildAction);
    }
}
