using System;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    public static class IStateflowsClientBuilderExtensions
    {
        [Obsolete("This method will be removed in a future release.")]
        public static IStateflowsClientBuilder SetEnvironment(this IStateflowsClientBuilder stateflowsClientBuilder, string environment)
        {
            BehaviorClassDefaults.CurrentEnvironment = environment;

            return stateflowsClientBuilder;
        }
        
        public static IStateflowsClientBuilder UseFullNamesFor(this IStateflowsClientBuilder stateflowsBuilder, TypedElements typedElements)
        {
            StateflowsSettings.FullNames = typedElements;

            return stateflowsBuilder;
        }
    }
}
