using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    public static class IStateflowsBuilderExtensions
    {
        public static IStateflowsBuilder SetEnvironment(this IStateflowsBuilder stateflowsBuilder, string environment)
        {
            (stateflowsBuilder as IStateflowsClientBuilder).SetEnvironment(environment);

            return stateflowsBuilder;
        }
    }
}
