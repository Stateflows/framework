using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class ReactiveStructuredActivityBuilderSpecialsPayloadExtensions
    {
        public static IReactiveStructuredActivityBuilder AddDataDecision<TTokenPayload>(this IReactiveStructuredActivityBuilder builder, string decisionNodeName, DecisionBuildAction<Token<TTokenPayload>> decisionBuildAction)
            => builder.AddTokenDecision<Token<TTokenPayload>>(decisionNodeName, decisionBuildAction);
    }
}
