using System;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    public static class IStateflowsBuilderExtensions
    {
        [Obsolete("This method will be removed in a future release.")]
        public static IStateflowsBuilder SetEnvironment(this IStateflowsBuilder stateflowsBuilder, string environment)
        {
            (stateflowsBuilder as IStateflowsClientBuilder).SetEnvironment(environment);

            return stateflowsBuilder;
        }
        
        public static IStateflowsBuilder UseFullNamesFor(this IStateflowsBuilder stateflowsBuilder, TypedElements typedElements)
        {
            (stateflowsBuilder as IStateflowsClientBuilder).UseFullNamesFor(typedElements);

            return stateflowsBuilder;
        }
    }
}
