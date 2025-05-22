using System;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    public static class IStateflowsClientBuilderExtensions
    {
        [Obsolete]
        public static IStateflowsClientBuilder SetEnvironment(this IStateflowsClientBuilder stateflowsClientBuilder, string environment)
        {
            BehaviorClassDefaults.CurrentEnvironment = environment;

            return stateflowsClientBuilder;
        }
    }
}
