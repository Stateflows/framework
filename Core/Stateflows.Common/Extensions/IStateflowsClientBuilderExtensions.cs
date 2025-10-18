using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    public static class IStateflowsClientBuilderExtensions
    {
        public static IStateflowsClientBuilder UseFullNamesFor(this IStateflowsClientBuilder stateflowsBuilder, TypedElements typedElements)
        {
            StateflowsSettings.FullNames = typedElements;

            return stateflowsBuilder;
        }
    }
}
